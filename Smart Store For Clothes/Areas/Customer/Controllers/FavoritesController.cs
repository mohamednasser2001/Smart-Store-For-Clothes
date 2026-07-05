using DataAccess.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Entities;
using Models.VM;

namespace Smart_Store_For_Clothes.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public FavoritesController(
            IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var favorites = _unitOfWork.FavoriteProducts.GetAll()
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            var productIds = favorites
                .Select(f => f.ProductId)
                .ToList();

            var products = _unitOfWork.Products.GetAll()
                .Where(p => productIds.Contains(p.Id))
                .ToList();

            var reviews = _unitOfWork.ProductReviews.GetAll().ToList();

            var productsVM = productIds
                .Select(id => products.FirstOrDefault(p => p.Id == id))
                .Where(p => p != null)
                .Select(p =>
                {
                    var productReviews = reviews.Where(r => r.ProductId == p!.Id).ToList();

                    return new ProductCardVM
                    {
                        Id = p!.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        ImageUrl = p.ImageUrl,
                        Gender = p.Gender,
                        CategoryName = _unitOfWork.Categories.GetById(p.CategoryId)?.Name ?? "No Category",

                        AverageRating = productReviews.Any()
                            ? productReviews.Average(r => r.Rating)
                            : 0,

                        ReviewsCount = productReviews.Count,

                        IsFavorite = true
                    };
                })
                .ToList();

            return View(productsVM);
        }

        [HttpPost]
        public IActionResult Toggle(int productId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new
                {
                    success = false,
                    message = "Please login first."
                });
            }

            var product = _unitOfWork.Products.GetById(productId);

            if (product == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Product not found."
                });
            }

            var existingFavorite = _unitOfWork.FavoriteProducts.GetAll()
                .FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);

            if (existingFavorite != null)
            {
                _unitOfWork.FavoriteProducts.Delete(existingFavorite);
                _unitOfWork.Save();

                return Json(new
                {
                    success = true,
                    isFavorite = false,
                    message = "Removed from favorites."
                });
            }

            FavoriteProduct favoriteProduct = new FavoriteProduct
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.Now
            };

            _unitOfWork.FavoriteProducts.Add(favoriteProduct);
            _unitOfWork.Save();

            return Json(new
            {
                success = true,
                isFavorite = true,
                message = "Added to favorites."
            });
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var favorite = _unitOfWork.FavoriteProducts.GetAll()
                .FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);

            if (favorite != null)
            {
                _unitOfWork.FavoriteProducts.Delete(favorite);
                _unitOfWork.Save();

                TempData["success"] = "Product removed from favorites.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
