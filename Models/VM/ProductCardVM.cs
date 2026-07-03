using System;

namespace Models.VM
{
    public class ProductCardVM
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public string CategoryName { get; set; } = null!;

        public string Gender { get; set; } = null!;

        public double AverageRating { get; set; }

        public int ReviewsCount { get; set; }
    }
}