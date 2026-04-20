using System;
using System.Collections.Generic;
using System.Text;
using Models.Entities;

namespace DataAccess.Repositories.IRepositories
{
    public interface IUnitOfWork
    {
        IRepository<Category> Categories { get; }
        IRepository<Product> Products { get; }

        IRepository<Size> Sizes { get; }
        IRepository<ProductSize> ProductSizes { get; }
        IRepository<SizeRecommendationRule> SizeRecommendationRules { get; }
        IRepository<Cart> Carts { get; }
        IRepository<CartItem> CartItems { get; }
        void Save();
    }
}
