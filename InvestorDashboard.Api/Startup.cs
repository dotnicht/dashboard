using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using InvestorDashboard.Api.Services;
using InvestorDashboard.Backend;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;
using OpenIddict.Core;
using OpenIddict.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace InvestorDashboard.Api
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Backend.Configuration.Configure(services, Configuration);
            DependencyInjection.Configure(services);

            services.AddAutoMapper(typeof(DependencyInjection), GetType());

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("InvestorDashboard.Backend"));
                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });

            // add identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(config => config.SignIn.RequireConfirmedEmail = true)
              .AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();

            // Configure Identity options and password complexity here
            services.Configure<IdentityOptions>(options =>
            {
                // User settings
                options.User.RequireUniqueEmail = true;

                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
                options.Lockout.MaxFailedAccessAttempts = 10;

                

                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            // Register the OpenIddict services.
            services.AddOpenIddict(options =>
            {
                // Register the Entity Framework stores.
                options.AddEntityFrameworkCoreStores<ApplicationDbContext>();

                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                options.AddMvcBinders();

                // Enable the authorization, logout, token and userinfo endpoints.
                options.EnableAuthorizationEndpoint("/connect/authorize")
                  .EnableLogoutEndpoint("/connect/logout")
                  .EnableTokenEndpoint("/connect/token")
                  .EnableUserinfoEndpoint("/userinfo");

                // Note: the Mvc.Client sample only uses the code flow and the password flow, but you
                // can enable the other flows if you need to support implicit or client credentials.
                options.AllowAuthorizationCodeFlow()
                  .AllowPasswordFlow()
                  .AllowRefreshTokenFlow();

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
                // Mark the "profile" scope as a supported scope in the discovery document.
                options.RegisterScopes(OpenIdConnectConstants.Scopes.Profile);

                // Make the "client_id" parameter mandatory when sending a token request.
                options.RequireClientIdentification();

                // When request caching is enabled, authorization and logout requests
                // are stored in the distributed cache by OpenIddict and the user agent
                // is redirected to the same page with a single parameter (request_id).
                // This allows flowing large OpenID Connect requests even when using
                // an external authentication provider like Google, Facebook or Twitter.
                options.EnableRequestCaching();
            });

            services
                .AddAuthentication(options => options.DefaultAuthenticateScheme = options.DefaultChallengeScheme = OAuthValidationDefaults.AuthenticationScheme)
                .AddOAuthValidation();

            services.AddMvc();
            services.AddNodeServices();

            services.AddAuthorization();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            var options = new RewriteOptions().AddRedirectToHttps();

            app.UseRewriter(options);

            app.UseCors(builder => builder
              .AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());

            app.UseStaticFiles();

            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                     name: "default",
                     template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Seed the database with the sample applications.
            // Note: in a real world application, this step should be part of a setup script.
            InitializeAsync(app.ApplicationServices, CancellationToken.None).GetAwaiter().GetResult();
        }

        private async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            // Create a new service scope to ensure the database context is correctly disposed when this methods returns.
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();

                var manager = scope.ServiceProvider.GetRequiredService<OpenIddictApplicationManager<OpenIddictApplication>>();

                if (await manager.FindByClientIdAsync("ID", cancellationToken) == null)
                {
                    var application = new OpenIddictApplication
                    {
                        ClientId = "ID",
                        DisplayName = "Investor Dashboard",
                        PostLogoutRedirectUris = "http://localhost:49295/signout-callback-oidc",
                        RedirectUris = "http://localhost:49295/signin-oidc"
                    };

                    await manager.CreateAsync(application, "901564A5-E7FE-42CB-B10D-61EF6A8F3654", cancellationToken);
                }

                // To test this sample with Postman, use the following settings:
                //
                // * Authorization URL: http://localhost:49295/connect/authorize
                // * Access token URL: http://localhost:49295/connect/token
                // * Client ID: postman
                // * Client secret: [blank] (not used with public clients)
                // * Scope: openid email profile roles
                // * Grant type: authorization code
                // * Request access token locally: yes
                if (await manager.FindByClientIdAsync("postman", cancellationToken) == null)
                {
                    var application = new OpenIddictApplication
                    {
                        ClientId = "postman",
                        DisplayName = "Postman",
                        RedirectUris = "https://www.getpostman.com/oauth2/callback"
                    };

                    await manager.CreateAsync(application, cancellationToken);
                }
            }
        }

        public static void Main()
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseNLog()
                .Build();

            host.Run();
        }
    }
}
