using DataAccess.Repositories;
using DataAccess.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Models.Entities;
using Models.VM;
using System.IO;

namespace Smart_Store_For_Clothes.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoriesController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 10;

            var categoriesQuery = _unitOfWork.Categories.GetAll();

            int totalCount = categoriesQuery.Count();

            var categories = categoriesQuery
                .Select(c => new CategoryCardVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductsCount = c.Products.Count,
                    ImageUrl = c.ImageUrl
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            CategoriesIndexVM vm = new CategoriesIndexVM
            {
                Categories = categories,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateCategoryVM model)
        {
            if (ModelState.IsValid)
            {
                string? imagePath = null;

                if (model.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "categories");

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

                    imagePath = "/images/categories/" + fileName;
                }

                Category category = new Category
                {
                    Name = model.Name,
                    Description = model.Description,
                    ImageUrl = imagePath
                };

                _unitOfWork.Categories.Add(category);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _unitOfWork.Categories.GetById(id);

            if (category == null)
            {
                return NotFound();
            }

            CreateCategoryVM vm = new CreateCategoryVM
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ExistingImageUrl = category.ImageUrl
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CreateCategoryVM model)
        {
            if (ModelState.IsValid)
            {
                var categoryFromDb = _unitOfWork.Categories.GetById(model.Id);

                if (categoryFromDb == null)
                {
                    return NotFound();
                }

                categoryFromDb.Name = model.Name;
                categoryFromDb.Description = model.Description;

                if (model.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "categories");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    if (!string.IsNullOrEmpty(categoryFromDb.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, categoryFromDb.ImageUrl.TrimStart('/'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(fileStream);
                    }

                    categoryFromDb.ImageUrl = "/images/categories/" + fileName;
                }

                _unitOfWork.Categories.Update(categoryFromDb);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var categoryFromDb = _unitOfWork.Categories.GetById(id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(categoryFromDb.ImageUrl))
            {
                string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, categoryFromDb.ImageUrl.TrimStart('/'));

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _unitOfWork.Categories.Delete(categoryFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}