using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public string? TryOnGifUrl { get; set; }

        [Required]
        public string Gender { get; set; } = null!;

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;

        public ICollection<ProductSize> ProductSizes { get; set; } = new HashSet<ProductSize>();

        public ICollection<ProductImage> ProductImages { get; set; } = new HashSet<ProductImage>();

        public ICollection<ProductColor> ProductColors { get; set; } = new HashSet<ProductColor>();

        public ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
    }
}