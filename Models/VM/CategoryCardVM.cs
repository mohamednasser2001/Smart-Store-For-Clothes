using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Models.VM
{
    public class CategoryCardVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int ProductsCount { get; set; }
        public string? ImageUrl { get; set; }
    }
}
