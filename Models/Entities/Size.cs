using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Entities
{
    public class Size
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<ProductSize> ProductSizes { get; set; } = new HashSet<ProductSize>();
        public ICollection<SizeRecommendationRule> SizeRecommendationRules { get; set; } = new HashSet<SizeRecommendationRule>();

        public ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
    }
}
