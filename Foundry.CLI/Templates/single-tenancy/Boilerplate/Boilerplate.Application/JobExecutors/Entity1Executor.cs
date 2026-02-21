using Boilerplate.Domain.Interfaces.JobExecutors;

namespace Boilerplate.Application.JobExecutors
{
    public class Entity1Executor : IEntity1JobExecutor
    {
        public async Task ExecuteAsync(int entity1Id)
        {
            throw new NotImplementedException();
        }
    }
}
