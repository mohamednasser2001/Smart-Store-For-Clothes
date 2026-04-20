using System;
using System.Collections.Generic;
using System.Text;

namespace Models.VM
{
    public class CategoriesIndexVM
    {
        public List<CategoryCardVM> Categories { get; set; } = new List<CategoryCardVM>();

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }
    }
}
