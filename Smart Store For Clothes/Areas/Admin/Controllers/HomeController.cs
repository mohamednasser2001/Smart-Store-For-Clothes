using DataAccess.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Models.VM;

namespace Smart_Store_For_Clothes.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            DashboardVM dashboardVM = new DashboardVM
            {
                CategoriesCount = _unitOfWork.Categories.Count(),
                ProductsCount = _unitOfWork.Products.Count(),
                SizesCount = _unitOfWork.Sizes.Count()
            };

            return View(dashboardVM);
        }
    }
}
