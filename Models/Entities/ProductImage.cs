using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ImageUrl { get; set; } = null!;

        public int DisplayOrder { get; set; } = 0;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }
}