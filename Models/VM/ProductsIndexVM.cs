using System;
using System.Collections.Generic;
using System.Text;

namespace Models.VM
{
    public class ProductsIndexVM
    {
        public List<ProductCardVM> Products { get; set; } = new List<ProductCardVM>();

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }
    }
}
