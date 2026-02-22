using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Interfaces.Repositories;
using Boilerplate.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        public async Task AddRangeAsync(List<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDelete(T obj)
        {
            obj.IsDeleted = true;
            _dbSet.Update(obj);
            await _context.SaveChangesAsync();
        }

        public IQueryable<T> GetAll(params Expression<Func<T, object>>[]? includes)
        {
            IQueryable<T> query = _dbSet;
            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return query.Where(x => !x.IsDeleted).AsQueryable();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
