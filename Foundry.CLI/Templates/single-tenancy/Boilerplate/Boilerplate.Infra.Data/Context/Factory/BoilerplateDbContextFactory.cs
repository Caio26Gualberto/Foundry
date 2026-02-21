using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Boilerplate.Infra.Data.Context.Factory
{
    public class BoilerplateDbContextFactory : IDesignTimeDbContextFactory<BoilerplateDbContext>
    {
        public BoilerplateDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetParent(Directory.GetCurrentDirectory())!.FullName;
            var configurationPath = Path.Combine(basePath, "Boilerplate.Api");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(configurationPath)
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<BoilerplateDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString, b =>
            {
                b.MigrationsAssembly(typeof(BoilerplateDbContext).Assembly.FullName);
                b.MigrationsHistoryTable("__EFMigrationsHistory", "core");
            });

            var currentUserContext = new DesignTimeCurrentContextService();

            return new BoilerplateDbContext(builder.Options, currentUserContext);
        }
    }
}
