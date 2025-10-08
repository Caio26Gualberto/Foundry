using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Boilerplate.Infra.Data.Context.Factory
{
    internal class BoilerplateDbContextFactory : IDesignTimeDbContextFactory<BoilerplateDbContext>
    {
        public BoilerplateDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("BoilerplateDb");

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Environment variable 'BoilerplateDb' not set.");

            var optionsBuilder = new DbContextOptionsBuilder<BoilerplateDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new BoilerplateDbContext(optionsBuilder.Options);
        }
    }
}
