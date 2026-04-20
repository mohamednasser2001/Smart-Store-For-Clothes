using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
}
