
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
        public DbSet<UserScrobble> Scrobble { get; set; }
        public DbSet<TrackLastfmSpotifyMapping> TrackMapping { get; set; }
        public DbSet<AlbumLastfmSpotifyMapping> AlbumMapping { get; set; }
        public DbSet<ArtistLastfmSpotifyMapping> ArtistMapping { get; set; }

        public DbSet<SpotifyListen> SpotifyListen { get; set; }

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

            modelBuilder.HasCollation("case_insensitive", locale: "en-u-ks-primary", provider: "icu", deterministic: false);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.SpotifyIsLinked)
                .IsRequired();
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.LastFmUsername)
                .UseCollation("case_insensitive");

            modelBuilder.Entity<Watcher>()
                .HasOne(w => w.User)
                .WithMany(u => u.Watchers)
                .HasForeignKey(w => w.UserId);

            modelBuilder.Entity<UserScrobble>()
                .HasOne(w => w.User)
                .WithMany(u => u.Scrobbles)
                .HasForeignKey(w => w.UserId);
            modelBuilder.Entity<UserScrobble>()
                .Property(s => s.TrackName)
                .UseCollation("case_insensitive");
            modelBuilder.Entity<UserScrobble>()
                .Property(s => s.AlbumName)
                .UseCollation("case_insensitive");
            modelBuilder.Entity<UserScrobble>()
                .Property(s => s.ArtistName)
                .UseCollation("case_insensitive");
            //modelBuilder.Entity<UserScrobble>()
            //    .HasIndex(x => new { x.UserId, x.ArtistName, x.TrackName });

            modelBuilder.Entity<TrackLastfmSpotifyMapping>().HasKey(s => s.SpotifyUri);
            modelBuilder.Entity<TrackLastfmSpotifyMapping>()
                .Property(s => s.LastfmTrackName)
                .UseCollation("case_insensitive");
            modelBuilder.Entity<TrackLastfmSpotifyMapping>()
                .Property(s => s.LastfmArtistName)
                .UseCollation("case_insensitive");

            modelBuilder.Entity<AlbumLastfmSpotifyMapping>().HasKey(s => s.SpotifyUri);
            modelBuilder.Entity<AlbumLastfmSpotifyMapping>()
                .Property(s => s.LastfmAlbumName)
                .UseCollation("case_insensitive");
            modelBuilder.Entity<AlbumLastfmSpotifyMapping>()
                .Property(s => s.LastfmArtistName)
                .UseCollation("case_insensitive");

            modelBuilder.Entity<ArtistLastfmSpotifyMapping>().HasKey(s => s.SpotifyUri);
            modelBuilder.Entity<ArtistLastfmSpotifyMapping>()
                .Property(s => s.LastfmArtistName)
                .UseCollation("case_insensitive");

            modelBuilder.Entity<SpotifyListen>().HasKey(s => s.Id);
            modelBuilder.Entity<SpotifyListen>()
                .Property(s => s.TrackName)
                .UseCollation("case_insensitive");
            modelBuilder.Entity<SpotifyListen>()
                .Property(s => s.AlbumName)
                .UseCollation("case_insensitive");
            modelBuilder.Entity<SpotifyListen>()
                .Property(s => s.ArtistName)
                .UseCollation("case_insensitive");
            //modelBuilder.Entity<SpotifyListen>()
            //    .HasIndex(x => new { x.UserId, x.ArtistName, x.TrackName });

            SeedData.Seed(modelBuilder);
        }

        public void CreatePlayerWatcher(string userId)
        {
            if(Watcher.Any(w => w.UserId == userId && w.Type == WatcherType.Player))
            {
                Logger.LogWarning("Trying to create more than one player watcher for user [{id}]", userId);
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
        private static string GetPath(string env) => $"{@Directory.GetCurrentDirectory()}/../Selector.Web/appsettings.{env}.json";

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            string configFile;

            if(File.Exists(GetPath("Development")))
            {
                configFile = GetPath("Development");
            }
            else if(File.Exists(GetPath("Production")))
            {
                configFile = GetPath("Production");
            }
            else
            {
                throw new FileNotFoundException("No config file available to load a connection string from");
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFile)
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseNpgsql(configuration.GetConnectionString("Default"));
            
            return new ApplicationDbContext(builder.Options, NullLogger<ApplicationDbContext>.Instance);
        }
    }
}