using System;
using System.Collections.Generic;

namespace Models.VM
{
    public class CustomerProductDetailsVM
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? RecommendedSize { get; set; }

        public List<ProductSizeItemVM> Sizes { get; set; } = new List<ProductSizeItemVM>();

        public List<ProductReviewVM> Reviews { get; set; } = new();

        public int NewRating { get; set; }

        public string? NewComment { get; set; }

        public double AverageRating { get; set; }

        public int ReviewsCount { get; set; }

        public List<ProductCardVM> RecommendedProducts { get; set; } = new();
    }
}