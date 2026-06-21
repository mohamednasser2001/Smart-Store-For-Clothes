using DataAccess.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Entities;
using Models.VM;

namespace Smart_Store_For_Clothes.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;


        public HomeController(IUnitOfWork unitOfWork,
                        UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var products = _unitOfWork.Products.GetAll().ToList();
            var reviews = _unitOfWork.ProductReviews.GetAll().ToList();

            var productsVM = products.Select(p =>
            {
                var productReviews = reviews.Where(r => r.ProductId == p.Id).ToList();

                return new ProductCardVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Gender = p.Gender,
                    CategoryName = _unitOfWork.Categories.GetById(p.CategoryId)?.Name ?? "No Category",

                    // ⭐ Reviews Aggregation
                    AverageRating = productReviews.Any()
                        ? productReviews.Average(r => r.Rating)
                        : 0,

                    ReviewsCount = productReviews.Count
                };
            }).ToList();

            CustomerProductFilterVM vm = new CustomerProductFilterVM
            {
                CategoriesList = _unitOfWork.Categories.GetAll()
                    .Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    })
                    .ToList(),

                Products = productsVM
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult FilterProducts(int? height, int? weight, int? age, int? categoryId, string? gender)
        {
            var reviews = _unitOfWork.ProductReviews.GetAll().ToList();
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
            if (!string.IsNullOrEmpty(gender))
            {
                productsQuery = productsQuery
                    .Where(p => p.Gender == gender)
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
            .Select(p =>
            {
                var productReviews = reviews.Where(r => r.ProductId == p.Id).ToList();

                return new ProductCardVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Gender = p.Gender,
                    CategoryName = _unitOfWork.Categories.GetById(p.CategoryId)?.Name ?? "No Category",

                    // ⭐ FIXED
                    AverageRating = productReviews.Any()
                        ? productReviews.Average(r => r.Rating)
                        : 0,

                    ReviewsCount = productReviews.Count
                };
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

            // =========================
            // Reviews Section
            // =========================

            var reviewEntities = _unitOfWork.ProductReviews.GetAll()
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            var userIds = reviewEntities
                .Select(r => r.UserId)
                .Distinct()
                .ToList();

            var users = _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(
                    u => u.Id,
                    u => u.UserName ?? u.Email ?? "User"
                );

            var reviews = reviewEntities.Select(r => new ProductReviewVM
            {
                UserName = users.ContainsKey(r.UserId) ? users[r.UserId] : "User",
                Comment = r.Comment ?? "",
                Rating = r.Rating,
                CreatedAt = r.CreatedAt
            }).ToList();

            var averageRating = reviews.Any()
                ? reviews.Average(r => r.Rating)
                : 0;

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
                }).ToList(),

                Reviews = reviews,
                AverageRating = averageRating,
                ReviewsCount = reviews.Count
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(int productId, string comment, int rating)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Details", new { id = productId });
            }

            if (string.IsNullOrWhiteSpace(comment) || rating < 1 || rating > 5)
            {
                TempData["ReviewError"] = "Please write a comment and select a valid rating.";
                return RedirectToAction("Details", new { id = productId });
            }

            var existingReview = _unitOfWork.ProductReviews.GetAll()
                .FirstOrDefault(r => r.ProductId == productId && r.UserId == userId);

            if (existingReview != null)
            {
                TempData["ReviewError"] = "You already reviewed this product.";
                return RedirectToAction("Details", new { id = productId });
            }

            var review = new ProductReview
            {
                ProductId = productId,
                UserId = userId,
                Comment = comment,
                Rating = rating,
                CreatedAt = DateTime.Now
            };

            _unitOfWork.ProductReviews.Add(review);
            _unitOfWork.Save();

            TempData["ReviewSuccess"] = "Your review has been added successfully.";

            return RedirectToAction("Details", new { id = productId });
        }
    }


}
