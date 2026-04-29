using System;
using System.Collections.Generic;
using System.Text;
using DataAccess.Data;
using DataAccess.Repositories.IRepositories;
using Models.Entities;

namespace DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IRepository<Category> Categories { get; private set; }
        public IRepository<Product> Products { get; private set; }
        public IRepository<Size> Sizes { get; private set; }
        public IRepository<ProductSize> ProductSizes { get; private set; }
        public IRepository<SizeRecommendationRule> SizeRecommendationRules { get; private set; }
        public IRepository<Cart> Carts { get; private set; }
        public IRepository<CartItem> CartItems { get; private set; }
        public IRepository<Order> Orders { get; private set; }
        public IRepository<OrderItem> OrderItems { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Categories = new Repository<Category>(_context);
            Products = new Repository<Product>(_context);
            Sizes = new Repository<Size>(_context);
            ProductSizes= new Repository<ProductSize>(_context);
            SizeRecommendationRules = new Repository<SizeRecommendationRule>(_context);
            Carts = new Repository<Cart>(_context);
            CartItems = new Repository<CartItem>(_context);
            Orders = new Repository<Order>(_context);
            OrderItems = new Repository<OrderItem>(_context);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
