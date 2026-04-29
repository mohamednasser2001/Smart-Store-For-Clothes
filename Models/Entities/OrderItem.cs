using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models.Entities
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int SizeId { get; set; }
        public Size Size { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
