using System;
using System.Collections.Generic;
using System.Text;

namespace Models.VM
{
    public class ProductReviewVM
    {
        public string UserName { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
