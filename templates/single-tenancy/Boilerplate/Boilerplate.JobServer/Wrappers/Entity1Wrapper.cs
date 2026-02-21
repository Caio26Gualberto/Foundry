using Boilerplate.Application.JobScheduler;
using Boilerplate.JobServer.Triggers;
using Hangfire;

namespace Boilerplate.JobServer.Wrappers
{
    public class Entity1Wrapper : IEntity1JobScheduler
    {
        public void EnqueueTask(int entity1Id)
        {
            BackgroundJob.Enqueue<Entity1Trigger>(x => x.ProcessEntity1Task(entity1Id));
        }
    }
}
