using DataAccess.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.VM;

namespace Smart_Store_For_Clothes.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            CustomerProductFilterVM vm = new CustomerProductFilterVM
            {
                CategoriesList = _unitOfWork.Categories.GetAll()
                    .Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    })
                    .ToList(),

                Products = _unitOfWork.Products.GetAll()
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
                    .ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult FilterProducts(int? height, int? weight, int? age, int? categoryId)
        {
            string? recommendedSize = null;

            int? recommendedSizeId = null;

            if (height.HasValue && weight.HasValue && age.HasValue)
            {
                var rule = _unitOfWork.SizeRecommendationRules.GetAll()
                    .FirstOrDefault(r =>
                        height.Value >= r.MinHeight && height.Value <= r.MaxHeight &&
                        weight.Value >= r.MinWeight && weight.Value <= r.MaxWeight &&
                        age.Value >= r.MinAge && age.Value <= r.MaxAge);

                if (rule != null)
                {
                    recommendedSizeId = rule.SizeId;
                    recommendedSize = _unitOfWork.Sizes.GetById(rule.SizeId)?.Name;
                }
            }

            var productsQuery = _unitOfWork.Products.GetAll().ToList();

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery
                    .Where(p => p.CategoryId == categoryId.Value)
                    .ToList();
            }

            if (recommendedSizeId.HasValue)
            {
                var productIdsWithSize = _unitOfWork.ProductSizes.GetAll()
                    .Where(ps => ps.SizeId == recommendedSizeId.Value && ps.QuantityInStock > 0)
                    .Select(ps => ps.ProductId)
                    .Distinct()
                    .ToList();

                productsQuery = productsQuery
                    .Where(p => productIdsWithSize.Contains(p.Id))
                    .ToList();
            }

            var products = productsQuery
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

            return Json(new
            {
                recommendedSize,
                products
            });
        }

        [HttpGet]
        public IActionResult Details(int id, int? height, int? weight, int? age)
        {
            var product = _unitOfWork.Products.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            var productSizes = _unitOfWork.ProductSizes.GetAll()
                .Where(ps => ps.ProductId == id && ps.QuantityInStock > 0)
                .ToList();

            string? recommendedSize = null;

            if (height.HasValue && weight.HasValue && age.HasValue)
            {
                var rule = _unitOfWork.SizeRecommendationRules.GetAll()
                    .FirstOrDefault(r =>
                        height.Value >= r.MinHeight && height.Value <= r.MaxHeight &&
                        weight.Value >= r.MinWeight && weight.Value <= r.MaxWeight &&
                        age.Value >= r.MinAge && age.Value <= r.MaxAge);

                if (rule != null)
                {
                    recommendedSize = _unitOfWork.Sizes.GetById(rule.SizeId)?.Name;
                }
            }

            CustomerProductDetailsVM vm = new CustomerProductDetailsVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CategoryName = _unitOfWork.Categories.GetById(product.CategoryId)?.Name ?? "No Category",
                RecommendedSize = recommendedSize,
                Sizes = productSizes.Select(ps => new ProductSizeItemVM
                {
                    SizeId = ps.SizeId,
                    SizeName = _unitOfWork.Sizes.GetById(ps.SizeId)?.Name ?? "N/A",
                    QuantityInStock = ps.QuantityInStock,
                    IsSelected = false
                }).ToList()
            };

            return View(vm);
        }
    }


}
