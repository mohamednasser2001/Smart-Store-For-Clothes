using System;
using System.Collections.Generic;
using System.Text;
using Models.Entities;

namespace Models.VM
{
    public class OrderDetailsVM
    {
        public Order Order { get; set; } = null!;
        public List<OrderItemDetailsVM> OrderItems { get; set; } = new List<OrderItemDetailsVM>();
    }
}
