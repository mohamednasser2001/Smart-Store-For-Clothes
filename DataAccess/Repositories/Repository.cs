using System;
using System.Collections.Generic;
using System.Text;
using DataAccess.Data;
using DataAccess.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public T? GetById(int id)
        {
            return _dbSet.Find(id);
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
            
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
          
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
           
        }
        public int Count()
        {
            return _dbSet.Count();
        }
    }
}
