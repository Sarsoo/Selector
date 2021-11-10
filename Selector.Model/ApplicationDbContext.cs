
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Selector.Model
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ILogger<ApplicationDbContext> Logger;

        public DbSet<Watcher> Watcher { get; set; }

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options, 
            ILogger<ApplicationDbContext> logger
        ) : base(options)
        {
            Logger = logger;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Watcher>()
                .HasOne(w => w.User)
                .WithMany(u => u.Watchers)
                .HasForeignKey(w => w.UserId);

            SeedData.Seed(modelBuilder);
        }

        public void CreatePlayerWatcher(string userId)
        {
            if(Watcher.Any(w => w.UserId == userId && w.Type == WatcherType.Player))
            {
                Logger.LogWarning($"Trying to create more than one player watcher for user [{userId}]");
                return;
            }

            Watcher.Add(new Watcher {
                UserId = userId,
                Type = WatcherType.Player
            });

            SaveChanges();
        }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@Directory.GetCurrentDirectory() + "/../Selector.Web/appsettings.Development.json")
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseNpgsql(configuration.GetConnectionString("Default"));
            
            return new ApplicationDbContext(builder.Options, NullLogger<ApplicationDbContext>.Instance);
        }
    }
}