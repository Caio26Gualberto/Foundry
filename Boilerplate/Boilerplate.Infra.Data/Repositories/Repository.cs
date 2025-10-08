using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.Repositories;
using Boilerplate.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Boilerplate.Infra.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : EntityBase
    {
        protected readonly BoilerplateDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(BoilerplateDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task SoftDelete(T obj)
        {
            obj.IsDeleted = true;
            _dbSet.Update(obj);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
