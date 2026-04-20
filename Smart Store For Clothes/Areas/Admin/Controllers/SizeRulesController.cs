using DataAccess.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Entities;
using Models.VM;
namespace Smart_Store_For_Clothes.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SizeRulesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SizeRulesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var rules = _unitOfWork.SizeRecommendationRules.GetAll()
                .ToList()
                .Select(rule => new SizeRuleCardVM
                {
                    Id = rule.Id,
                    SizeName = _unitOfWork.Sizes.GetById(rule.SizeId)?.Name ?? "No Size",
                    MinHeight = rule.MinHeight,
                    MaxHeight = rule.MaxHeight,
                    MinWeight = rule.MinWeight,
                    MaxWeight = rule.MaxWeight,
                    MinAge = rule.MinAge,
                    MaxAge = rule.MaxAge
                })
                .ToList();

            return View(rules);
        }

        [HttpGet]
        public IActionResult Create()
        {
            SizeRuleVM vm = new SizeRuleVM
            {
                SizesList = _unitOfWork.Sizes.GetAll()
                    .Select(s => new SelectListItem
                    {
                        Text = s.Name,
                        Value = s.Id.ToString()
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SizeRuleVM model)
        {
            if (ModelState.IsValid)
            {
                SizeRecommendationRule rule = new SizeRecommendationRule
                {
                    MinHeight = model.MinHeight,
                    MaxHeight = model.MaxHeight,
                    MinWeight = model.MinWeight,
                    MaxWeight = model.MaxWeight,
                    MinAge = model.MinAge,
                    MaxAge = model.MaxAge,
                    SizeId = model.SizeId
                };

                _unitOfWork.SizeRecommendationRules.Add(rule);
                _unitOfWork.Save();

                TempData["success"] = "Size rule created successfully";
                return RedirectToAction(nameof(Index));
            }

            model.SizesList = _unitOfWork.Sizes.GetAll()
                .Select(s => new SelectListItem
                {
                    Text = s.Name,
                    Value = s.Id.ToString()
                })
                .ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var rule = _unitOfWork.SizeRecommendationRules.GetById(id);

            if (rule == null)
                return NotFound();

            SizeRuleVM vm = new SizeRuleVM
            {
                Id = rule.Id,
                MinHeight = rule.MinHeight,
                MaxHeight = rule.MaxHeight,
                MinWeight = rule.MinWeight,
                MaxWeight = rule.MaxWeight,
                MinAge = rule.MinAge,
                MaxAge = rule.MaxAge,
                SizeId = rule.SizeId,

                SizesList = _unitOfWork.Sizes.GetAll()
                    .Select(s => new SelectListItem
                    {
                        Text = s.Name,
                        Value = s.Id.ToString()
                    }).ToList()
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SizeRuleVM model)
        {
            if (ModelState.IsValid)
            {
                var rule = _unitOfWork.SizeRecommendationRules.GetById(model.Id);

                if (rule == null)
                    return NotFound();

                rule.MinHeight = model.MinHeight;
                rule.MaxHeight = model.MaxHeight;
                rule.MinWeight = model.MinWeight;
                rule.MaxWeight = model.MaxWeight;
                rule.MinAge = model.MinAge;
                rule.MaxAge = model.MaxAge;
                rule.SizeId = model.SizeId;

                _unitOfWork.Save();

                TempData["success"] = "Rule updated successfully";
                return RedirectToAction(nameof(Index));
            }

            model.SizesList = _unitOfWork.Sizes.GetAll()
                .Select(s => new SelectListItem
                {
                    Text = s.Name,
                    Value = s.Id.ToString()
                }).ToList();

            return View(model);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var rule = _unitOfWork.SizeRecommendationRules.GetById(id);

            if (rule == null)
                return Json(new { success = false });

            _unitOfWork.SizeRecommendationRules.Delete(rule);
            _unitOfWork.Save();

            return Json(new { success = true });
        }
    }
}
