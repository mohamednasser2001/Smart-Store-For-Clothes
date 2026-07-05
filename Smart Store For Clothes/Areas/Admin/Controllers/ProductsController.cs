using DataAccess.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Entities;
using Models.VM;

namespace Smart_Store_For_Clothes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 10;

            var productsQuery = _unitOfWork.Products.GetAll();

            int totalCount = productsQuery.Count();

            var products = productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(p => new ProductCardVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Gender = p.Gender,
                    CategoryName = _unitOfWork.Categories.GetById(p.CategoryId)?.Name ?? "No Category"
                })
                .ToList();

            ProductsIndexVM vm = new ProductsIndexVM
            {
                Products = products,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            CreateProductVM vm = new CreateProductVM
            {
                CategoriesList = GetCategoriesList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateProductVM model)
        {
            if (ModelState.IsValid)
            {
                string imagePath = "";
                string? tryOnVideoPath = null;

                if (model.ImageFile != null)
                {
                    imagePath = UploadProductImage(model.ImageFile);
                }

                if (model.TryOnGifFile != null)
                {
                    tryOnVideoPath = UploadTryOnVideo(model.TryOnGifFile);
                }

                Product product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    Gender = model.Gender,
                    ImageUrl = imagePath,
                    TryOnGifUrl = tryOnVideoPath
                };

                _unitOfWork.Products.Add(product);
                _unitOfWork.Save();

                UploadAdditionalProductImages(product.Id, model.AdditionalImages);
                SaveProductColors(product.Id, model.ColorsText);

                _unitOfWork.Save();

                TempData["success"] = "Product created successfully";
                return RedirectToAction(nameof(Index));
            }

            model.CategoriesList = GetCategoriesList();
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _unitOfWork.Products.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            var existingImages = _unitOfWork.ProductImages.GetAll()
                .Where(pi => pi.ProductId == id)
                .OrderBy(pi => pi.DisplayOrder)
                .ToList();

            var existingColors = _unitOfWork.ProductColors.GetAll()
                .Where(pc => pc.ProductId == id)
                .OrderBy(pc => pc.ColorName)
                .ToList();

            CreateProductVM vm = new CreateProductVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                Gender = product.Gender,
                ExistingImageUrl = product.ImageUrl,
                ExistingTryOnGifUrl = product.TryOnGifUrl,
                ExistingProductImages = existingImages,
                ExistingProductColors = existingColors,
                ColorsText = string.Join(", ", existingColors.Select(c => c.ColorName)),
                CategoriesList = GetCategoriesList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CreateProductVM model)
        {
            if (ModelState.IsValid)
            {
                var product = _unitOfWork.Products.GetById(model.Id);

                if (product == null)
                {
                    return NotFound();
                }

                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;
                product.Gender = model.Gender;

                if (model.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        DeleteFile(product.ImageUrl);
                    }

                    product.ImageUrl = UploadProductImage(model.ImageFile);
                }

                if (model.TryOnGifFile != null)
                {
                    if (!string.IsNullOrEmpty(product.TryOnGifUrl))
                    {
                        DeleteFile(product.TryOnGifUrl);
                    }

                    product.TryOnGifUrl = UploadTryOnVideo(model.TryOnGifFile);
                }

                UploadAdditionalProductImages(product.Id, model.AdditionalImages);
                SaveProductColors(product.Id, model.ColorsText);

                _unitOfWork.Products.Update(product);
                _unitOfWork.Save();

                TempData["success"] = "Product updated successfully";
                return RedirectToAction(nameof(Index));
            }

            model.CategoriesList = GetCategoriesList();

            model.ExistingProductImages = _unitOfWork.ProductImages.GetAll()
                .Where(pi => pi.ProductId == model.Id)
                .OrderBy(pi => pi.DisplayOrder)
                .ToList();

            model.ExistingProductColors = _unitOfWork.ProductColors.GetAll()
                .Where(pc => pc.ProductId == model.Id)
                .OrderBy(pc => pc.ColorName)
                .ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProductImage(int imageId, int productId)
        {
            var image = _unitOfWork.ProductImages.GetById(imageId);

            if (image == null)
            {
                TempData["error"] = "Image not found.";
                return RedirectToAction(nameof(Edit), new { id = productId });
            }

            if (!string.IsNullOrEmpty(image.ImageUrl))
            {
                DeleteFile(image.ImageUrl);
            }

            _unitOfWork.ProductImages.Delete(image);
            _unitOfWork.Save();

            TempData["success"] = "Product image deleted successfully";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var productFromDb = _unitOfWork.Products.GetById(id);

            if (productFromDb == null)
            {
                return NotFound();
            }

            bool productHasOrders = _unitOfWork.OrderItems.GetAll()
                .Any(oi => oi.ProductId == id);

            if (productHasOrders)
            {
                TempData["error"] = "Cannot delete this product because it is already used in orders.";
                return RedirectToAction(nameof(Index));
            }

            var productSizes = _unitOfWork.ProductSizes.GetAll()
                .Where(ps => ps.ProductId == id)
                .ToList();

            foreach (var productSize in productSizes)
            {
                _unitOfWork.ProductSizes.Delete(productSize);
            }

            var cartItems = _unitOfWork.CartItems.GetAll()
                .Where(ci => ci.ProductId == id)
                .ToList();

            foreach (var cartItem in cartItems)
            {
                _unitOfWork.CartItems.Delete(cartItem);
            }

            var reviews = _unitOfWork.ProductReviews.GetAll()
                .Where(r => r.ProductId == id)
                .ToList();

            foreach (var review in reviews)
            {
                _unitOfWork.ProductReviews.Delete(review);
            }

            var favorites = _unitOfWork.FavoriteProducts.GetAll()
                .Where(f => f.ProductId == id)
                .ToList();

            foreach (var favorite in favorites)
            {
                _unitOfWork.FavoriteProducts.Delete(favorite);
            }

            var productImages = _unitOfWork.ProductImages.GetAll()
                .Where(pi => pi.ProductId == id)
                .ToList();

            foreach (var productImage in productImages)
            {
                if (!string.IsNullOrEmpty(productImage.ImageUrl))
                {
                    DeleteFile(productImage.ImageUrl);
                }

                _unitOfWork.ProductImages.Delete(productImage);
            }

            var productColors = _unitOfWork.ProductColors.GetAll()
                .Where(pc => pc.ProductId == id)
                .ToList();

            foreach (var productColor in productColors)
            {
                _unitOfWork.ProductColors.Delete(productColor);
            }

            if (!string.IsNullOrEmpty(productFromDb.ImageUrl))
            {
                DeleteFile(productFromDb.ImageUrl);
            }

            if (!string.IsNullOrEmpty(productFromDb.TryOnGifUrl))
            {
                DeleteFile(productFromDb.TryOnGifUrl);
            }

            _unitOfWork.Products.Delete(productFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Product deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ManageSizes(int id)
        {
            var product = _unitOfWork.Products.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            var allSizes = _unitOfWork.Sizes.GetAll().ToList();

            var productSizes = _unitOfWork.ProductSizes.GetAll()
                .Where(ps => ps.ProductId == id)
                .ToList();

            ManageProductSizesVM vm = new ManageProductSizesVM
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Sizes = allSizes.Select(size => new ProductSizeItemVM
                {
                    SizeId = size.Id,
                    SizeName = size.Name,
                    IsSelected = productSizes.Any(ps => ps.SizeId == size.Id),
                    QuantityInStock = productSizes
                        .FirstOrDefault(ps => ps.SizeId == size.Id)?.QuantityInStock ?? 0
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ManageSizes(ManageProductSizesVM model)
        {
            var product = _unitOfWork.Products.GetById(model.ProductId);

            if (product == null)
            {
                return NotFound();
            }

            var existingProductSizes = _unitOfWork.ProductSizes.GetAll()
                .Where(ps => ps.ProductId == model.ProductId)
                .ToList();

            foreach (var existing in existingProductSizes)
            {
                _unitOfWork.ProductSizes.Delete(existing);
            }

            foreach (var item in model.Sizes)
            {
                if (item.IsSelected)
                {
                    ProductSize productSize = new ProductSize
                    {
                        ProductId = model.ProductId,
                        SizeId = item.SizeId,
                        QuantityInStock = item.QuantityInStock
                    };

                    _unitOfWork.ProductSizes.Add(productSize);
                }
            }

            _unitOfWork.Save();

            TempData["success"] = "Product sizes updated successfully";
            return RedirectToAction(nameof(Index));
        }

        private IEnumerable<SelectListItem> GetCategoriesList()
        {
            return _unitOfWork.Categories.GetAll()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
        }

        private void UploadAdditionalProductImages(int productId, List<IFormFile> additionalImages)
        {
            if (additionalImages == null || !additionalImages.Any())
            {
                return;
            }

            int displayOrder = _unitOfWork.ProductImages.GetAll()
                .Where(pi => pi.ProductId == productId)
                .Select(pi => pi.DisplayOrder)
                .DefaultIfEmpty(0)
                .Max();

            foreach (var image in additionalImages)
            {
                if (image == null || image.Length == 0)
                {
                    continue;
                }

                displayOrder++;

                ProductImage productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = UploadProductGalleryImage(image),
                    DisplayOrder = displayOrder
                };

                _unitOfWork.ProductImages.Add(productImage);
            }
        }

        private void SaveProductColors(int productId, string? colorsText)
        {
            var existingColors = _unitOfWork.ProductColors.GetAll()
                .Where(pc => pc.ProductId == productId)
                .ToList();

            foreach (var color in existingColors)
            {
                _unitOfWork.ProductColors.Delete(color);
            }

            if (existingColors.Any())
            {
                _unitOfWork.Save();
            }

            if (string.IsNullOrWhiteSpace(colorsText))
            {
                return;
            }

            var colorNames = colorsText
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var colorName in colorNames)
            {
                ProductColor productColor = new ProductColor
                {
                    ProductId = productId,
                    ColorName = colorName
                };

                _unitOfWork.ProductColors.Add(productColor);
            }
        }

        private string UploadProductImage(IFormFile imageFile)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            return "/images/" + fileName;
        }

        private string UploadProductGalleryImage(IFormFile imageFile)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", "gallery");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            return "/images/products/gallery/" + fileName;
        }

        private string UploadTryOnVideo(IFormFile videoFile)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(videoFile.FileName);

            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "videos", "tryon-videos");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                videoFile.CopyTo(stream);
            }

            return "/videos/tryon-videos/" + fileName;
        }

        private void DeleteFile(string fileUrl)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, fileUrl.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}