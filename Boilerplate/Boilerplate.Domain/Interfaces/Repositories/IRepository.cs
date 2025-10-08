using Boilerplate.Domain.Entities;

namespace Boilerplate.Domain.Interfaces.Repositories
{
    public interface IRepository<T> where T : EntityBase
    {
        Task<T?> GetByIdAsync(int id);
        IQueryable<T> GetAll();
        Task AddAsync(T obj);
        Task UpdateAsync(T obj);
        Task DeleteAsync(T obj);
        Task SoftDelete(T obj);
    }
}
