using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.VM
{
    public class CustomerProductFilterVM
    {
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public int? Age { get; set; }

        public int? CategoryId { get; set; }

        public string? RecommendedSize { get; set; }

        public IEnumerable<SelectListItem> CategoriesList { get; set; } = new List<SelectListItem>();

        public List<ProductCardVM> Products { get; set; } = new List<ProductCardVM>();
    }
}
