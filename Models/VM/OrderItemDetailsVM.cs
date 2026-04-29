using System;
using System.Collections.Generic;
using System.Text;

namespace Models.VM
{
    public class OrderItemDetailsVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public int SizeId { get; set; }
        public string SizeName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
