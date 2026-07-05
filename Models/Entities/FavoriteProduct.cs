using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities
{
    public class FavoriteProduct
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}