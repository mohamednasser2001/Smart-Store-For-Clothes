using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Entities
{
    public class SizeRecommendationRule
    {
        public int Id { get; set; }

        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }

        public int MinWeight { get; set; }
        public int MaxWeight { get; set; }

        public int MinAge { get; set; }
        public int MaxAge { get; set; }

        public int SizeId { get; set; }
        public Size Size { get; set; } = null!;
    }
}
