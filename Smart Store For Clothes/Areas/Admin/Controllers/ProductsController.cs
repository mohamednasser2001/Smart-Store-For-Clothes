using DataAccess.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Entities;
using Models.VM;

namespace Smart_Store_For_Clothes.Areas.Admin.Controllers
{
    [Area("Admin")]
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
                CategoriesList = _unitOfWork.Categories.GetAll()
                    .Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateProductVM model)
        {
            if (ModelState.IsValid)
            {
                string? imagePath = null;

                if (model.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(fileStream);
                    }

                    imagePath = "/images/products/" + fileName;
                }

                Product product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    ImageUrl = imagePath
                };

                _unitOfWork.Products.Add(product);
                _unitOfWork.Save();

                TempData["success"] = "Product created successfully";
                return RedirectToAction(nameof(Index));
            }

            model.CategoriesList = _unitOfWork.Categories.GetAll()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
                .ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _unitOfWork.Products.GetById(id);

            if (product == null)
                return NotFound();

            CreateProductVM vm = new CreateProductVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ExistingImageUrl = product.ImageUrl,

                CategoriesList = _unitOfWork.Categories.GetAll()
                    .Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    })
                    .ToList()
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
                    return NotFound();

                // update fields
                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                // update image
                if (model.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(fileStream);
                    }

                    product.ImageUrl = "/images/products/" + fileName;
                }

                _unitOfWork.Products.Update(product);
                _unitOfWork.Save();

                TempData["success"] = "Product updated successfully";
                return RedirectToAction(nameof(Index));
            }

            model.CategoriesList = _unitOfWork.Categories.GetAll()
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
                .ToList();

            return View(model);
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

            if (!string.IsNullOrEmpty(productFromDb.ImageUrl))
            {
                string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productFromDb.ImageUrl.TrimStart('/'));

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
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


    }
}
