using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.VM
{
    public class CreateProductVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        // الخاصية اللي هتشيل القيمة المختارة من الـ Dropdown
        [Required(ErrorMessage = "Please select a gender")]
        public string Gender { get; set; } = null!;

        public string? ExistingImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        public IEnumerable<SelectListItem> CategoriesList { get; set; } = new List<SelectListItem>();

        
        public IEnumerable<SelectListItem> GenderList { get; set; } = new List<SelectListItem>
        {
        new SelectListItem { Value = "Men", Text = "Men" },
        new SelectListItem { Value = "Women", Text = "Women" },
        new SelectListItem { Value = "Boys", Text = "Boys" },
        new SelectListItem { Value = "Girls", Text = "Girls" }
        };
    }
}
