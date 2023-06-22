using JsonDataDownloader.Models;
using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace JsonDataDownloader
{
    public class JsonDbContext: DbContext
    {
        public DbSet<JsonData> JsonData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var connectionString = configuration.GetConnectionString("PgDbContextConnection");
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            optionsBuilder.UseNpgsql(builder.ConnectionString);
        }
    }
}
