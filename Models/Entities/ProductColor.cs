using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities
{
    public class ProductColor
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required]
        public string ColorName { get; set; } = null!;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }
}