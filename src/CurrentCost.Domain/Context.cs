#nullable enable
using CurrentCost.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrentCost.Domain
{
#pragma warning disable 8618
	public class Context : DbContext
	{
        private readonly ConnectionString _connectionString;

        public Context(DbContextOptions<Context> options, ConnectionString connectionString) : base(options)
        {
            _connectionString = connectionString;
        }

		public DbSet<Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString.GetAddress());
        }
	}
}
