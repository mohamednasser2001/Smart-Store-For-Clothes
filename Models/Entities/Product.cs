using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        // FK
        public int CategoryId { get; set; }

        // Navigation
        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;
        public ICollection<ProductSize> ProductSizes { get; set; } = new HashSet<ProductSize>();

        public ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
    }
}
