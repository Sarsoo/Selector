using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Selector.Cache.Extensions;
using Selector.Events;
using Selector.Extensions;
using Selector.Model;
using Selector.Model.Extensions;
using Selector.Spotify;
using Selector.Web.Auth;
using Selector.Web.Extensions;
using Selector.Web.Hubs;

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
            services.Configure<RootOptions>(options => { OptionsHelper.ConfigureOptions(options, Configuration); });
            services.Configure<RedisOptions>(options =>
            {
                Configuration.GetSection(string.Join(':', RootOptions.Key, RedisOptions.Key)).Bind(options);
            });
            services.Configure<NowPlayingOptions>(options =>
            {
                Configuration.GetSection(string.Join(':', RootOptions.Key, NowPlayingOptions.Key)).Bind(options);
            });
            services.Configure<JwtOptions>(options => { Configuration.GetSection(JwtOptions._Key).Bind(options); });
            services.Configure<AppleMusicOptions>(options =>
            {
                Configuration.GetSection(AppleMusicOptions._Key).Bind(options);
            });

            var config = OptionsHelper.ConfigureOptions(Configuration);

            services.Configure<SpotifyAppCredentials>(options =>
            {
                options.ClientId = config.ClientId;
                options.ClientSecret = config.ClientSecret;
            });

            services.AddRazorPages(o =>
                {
                    o.Conventions.AllowAnonymousToPage("/");
                    o.Conventions.AuthorizePage("/Now", AuthConstants.CookieAuthentication);
                    o.Conventions.AuthorizePage("/Past", AuthConstants.CookieAuthentication);
                    o.Conventions.AllowAnonymousToPage("/Privacy");
                    o.Conventions.AllowAnonymousToPage("/Error");
                    o.Conventions.AllowAnonymousToAreaPage("Identity", "/Login");
                    o.Conventions.AllowAnonymousToAreaPage("Identity", "/Logout");
                    o.Conventions.AllowAnonymousToAreaPage("Identity", "/Register");
                    o.Conventions.AllowAnonymousToAreaPage("Identity", "/AccessDenied");
                    o.Conventions.AllowAnonymousToAreaPage("Identity", "/Lockout");
                    o.Conventions.AuthorizeAreaPage("Identity", "/Manage", AuthConstants.CookieAuthentication);
                })
                .AddRazorRuntimeCompilation();
            services.AddControllers();
            services.AddSignalR(o => o.EnableDetailedErrors = true);
            services.AddHttpClient();

            ConfigureDB(services, config);
            ConfigureIdentity(services, config);
            ConfigureAuth(services, config);

            services.AddEvents();

            services.AddSpotify();
            ConfigureLastFm(config, services);

            ConfigureRedis(services, config);
        }

        public void ConfigureDB(IServiceCollection services, RootOptions config)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("Default"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "public"))
            );
            services.AddDBPlayCountPuller();
            services.AddTransient<IScrobbleRepository, ScrobbleRepository>()
                .AddTransient<ISpotifyListenRepository, SpotifyListenRepository>()
                .AddTransient<IAppleListenRepository, AppleListenRepository>();

            services.AddTransient<IListenRepository, MetaListenRepository>();
            //services.AddTransient<IListenRepository, SpotifyListenRepository>();
        }

        public void ConfigureIdentity(IServiceCollection services, RootOptions config)
        {
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
        }

        public void ConfigureAuth(IServiceCollection services, RootOptions config)
        {
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = config.CookieExpiry;

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            var jwtConfig = Configuration.GetSection(JwtOptions._Key).Get<JwtOptions>();

            services.AddAuthentication()
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.SaveToken = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = jwtConfig.Issuer,
                        ValidAudience = jwtConfig.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key))
                    };
                });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme,
                        JwtBearerDefaults.AuthenticationScheme)
                    .Build();

                options.AddPolicy(AuthConstants.CookieAuthentication, new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                    .Build());
            });

            services.AddTransient<JwtTokenService>();

            services.AddAuthorisationHandlers();
        }

        public void ConfigureRedis(IServiceCollection services, RootOptions config)
        {
            if (config.RedisOptions.Enabled)
            {
                Console.WriteLine("> Adding Redis...");
                services.AddRedisServices(config.RedisOptions.ConnectionString);

                Console.WriteLine("> Adding cache event maps...");

                services.AddTransient<IEventMapping, ToPubSub.SpotifyLink>();
                services.AddTransient<IEventMapping, ToPubSub.AppleMusicLink>();
                services.AddTransient<IEventMapping, ToPubSub.Lastfm>();
                services.AddTransient<IEventMapping, FromPubSub.NowPlaying>();

                services.AddCacheHubProxy();

                Console.WriteLine("> Adding caching Spotify consumers...");
                services.AddCachingSpotify();
            }
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
                // app.UseHttpsRedirection();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<NowPlayingHub>("/nowhub");
                endpoints.MapHub<PastHub>("/pasthub");
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