using System;
using System.Collections.Generic;
using System.Text;

namespace Models.VM
{
    public class ManageProductSizesVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;

        public List<ProductSizeItemVM> Sizes { get; set; } = new List<ProductSizeItemVM>();
    }
}
