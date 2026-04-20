using System;
using System.Collections.Generic;
using System.Text;

namespace Models.VM
{
    public class ProductSizeItemVM
    {
        public int SizeId { get; set; }
        public string SizeName { get; set; } = null!;
        public bool IsSelected { get; set; }
        public int QuantityInStock { get; set; }
    }
}
