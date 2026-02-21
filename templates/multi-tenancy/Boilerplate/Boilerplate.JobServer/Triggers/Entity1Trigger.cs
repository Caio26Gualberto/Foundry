using Boilerplate.Domain.Interfaces.JobExecutors;
using Hangfire;

namespace Boilerplate.JobServer.Triggers
{
    public class Entity1Trigger
    {
        private readonly IEntity1JobExecutor _entity1JobExecutor;
        public Entity1Trigger(IEntity1JobExecutor entity1JobExecutor)
        {
            _entity1JobExecutor = entity1JobExecutor;
        }

        [Queue("entity1")]
        [JobDisplayName("EntityJobId: {0}")]
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void ProcessEntity1Task(int entity1Id)
        {
            _entity1JobExecutor.ExecuteAsync(entity1Id);
        }
    }
}
