using Boilerplate.Domain.Entities;
using System.Linq.Expressions;

namespace Boilerplate.Domain.Interfaces.Repositories
{
    public interface IRepository<T> where T : EntityBase
    {
        Task<T?> GetByIdAsync(int id);
        IQueryable<T> GetAll(params Expression<Func<T, object>>[]? includes);
        Task AddAsync(T obj);
        Task AddRangeAsync(List<T> objs);
        Task UpdateAsync(T obj);
        Task DeleteAsync(T obj);
        Task SoftDelete(T obj);
    }
}
