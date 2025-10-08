using Boilerplate.Domain.Entities;

namespace Boilerplate.Domain.Interfaces.Repositories.IUnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<T> Repository<T>() where T : EntityBase;
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
