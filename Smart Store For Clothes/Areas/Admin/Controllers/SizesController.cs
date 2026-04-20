using DataAccess.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Models.Entities;

namespace Smart_Store_For_Clothes.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SizesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SizesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var sizes = _unitOfWork.Sizes.GetAll();
            return View(sizes);
        }

        
        public IActionResult Create()
        {
            return View();
        }

    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Size size)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Sizes.Add(size);
                _unitOfWork.Save();

                TempData["success"] = "Size created successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(size);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var size = _unitOfWork.Sizes.GetById(id);

            if (size == null)
            {
                return NotFound();
            }

            return View(size);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Size size)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Sizes.Update(size);
                _unitOfWork.Save();

                TempData["success"] = "Size updated successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(size);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var size = _unitOfWork.Sizes.GetById(id);

            if (size == null)
            {
                return NotFound();
            }

            _unitOfWork.Sizes.Delete(size);
            _unitOfWork.Save();

            TempData["success"] = "Size deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
