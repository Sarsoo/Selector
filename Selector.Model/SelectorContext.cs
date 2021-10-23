
using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Selector.Model
{

    public class SelectorContext : IdentityDbContext
    {
        public DbSet<Watcher> Watcher { get; set; }

        public SelectorContext(DbContextOptions<SelectorContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SelectorContext>
    {
        public SelectorContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@Directory.GetCurrentDirectory() + "/../Selector.Web/appsettings.Development.json")
                .Build();

            var builder = new DbContextOptionsBuilder<SelectorContext>();
            builder.UseNpgsql(configuration.GetConnectionString("Default"));
            
            return new SelectorContext(builder.Options);
        }
    }
}