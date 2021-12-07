using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Selector.Web.Hubs;
using Selector.Web.Extensions;
using Selector.Extensions;
using Selector.Model;
using Selector.Model.Extensions;
using Selector.Cache;
using Selector.Cache.Extensions;

namespace Selector.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RootOptions>(options =>
            {
                OptionsHelper.ConfigureOptions(options, Configuration);
            });
            var config = OptionsHelper.ConfigureOptions(Configuration);

            services.Configure<SpotifyAppCredentials>(options =>
            {
                options.ClientId = config.ClientId;
                options.ClientSecret = config.ClientSecret;
            }); 

            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddControllers();
            services.AddSignalR(o => o.EnableDetailedErrors = true);

            services.AddDbContext<ApplicationDbContext>(options => 
                options.UseNpgsql(Configuration.GetConnectionString("Default"))
            );

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(1);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddAuthorisationHandlers();

            if (config.RedisOptions.Enabled)
                services.AddRedisServices(config.RedisOptions.ConnectionString);

            services.AddSpotify();
            if (config.RedisOptions.Enabled)
            {
                Console.WriteLine("> Adding caching Spotify consumers...");
                services.AddCachingSpotify();
                services.AddCacheHubProxy();
            }

            ConfigureLastFm(config, services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<NowPlayingHub>("/hub");
            });
        }

        public static void ConfigureLastFm(RootOptions config, IServiceCollection services)
        {
            if (config.LastfmClient is not null)
            {
                Console.WriteLine("> Adding Last.fm credentials...");

                services.AddLastFm(config.LastfmClient, config.LastfmSecret);

                if (config.RedisOptions.Enabled)
                {
                    Console.WriteLine("> Adding caching Last.fm consumers...");
                    services.AddCachingLastFm();
                }
            }
            else
            {
                Console.WriteLine("> No Last.fm credentials, skipping init...");
            }
        }
    }
}
