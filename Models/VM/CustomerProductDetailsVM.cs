using System;
using System.Collections.Generic;
using Models.Entities;

namespace Models.VM
{
    public class CustomerProductDetailsVM
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public string? TryOnGifUrl { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? RecommendedSize { get; set; }

        public List<ProductSizeItemVM> Sizes { get; set; } = new List<ProductSizeItemVM>();

        public List<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

        public List<ProductColor> ProductColors { get; set; } = new List<ProductColor>();

        public List<ProductReviewVM> Reviews { get; set; } = new();

        public int NewRating { get; set; }

        public string? NewComment { get; set; }

        public double AverageRating { get; set; }

        public int ReviewsCount { get; set; }

        public List<ProductCardVM> RecommendedProducts { get; set; } = new();
    }
}