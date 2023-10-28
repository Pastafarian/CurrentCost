using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CurrentCost.Domain.Factory
{
	public class ContextFactory : IDesignTimeDbContextFactory<Context>
	{
        private readonly ConnectionString _connectionString;

        public ContextFactory(ConnectionString connectionString) => _connectionString = connectionString;

        public ContextFactory() => _connectionString = new ConnectionString
        {
            AppConnectionString = "Host=localhost;Port=5432;Username=guest;Password=guest;Database=CurrentCost",
            DockerAppConnectionString = "Host=current-cost_postgres;Port=5432;Username=guest;Password=guest;Database=CurrentCost"
        };

        public Context CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<Context>();

            return new Context(optionsBuilder.Options, _connectionString);
		}
	}
}
