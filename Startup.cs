using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Filters;
using AspNet.Security.OpenIdConnect.Primitives;
using OpenIddict.Abstractions;
using WEB.Models;

namespace WEB
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        // for debugging
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
        }
    }

    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });


            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

                options.UseOpenIddict();
            });

            services.AddIdentity<User, AppRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // this should only be required if the Authorize is missing AuthenticationSchemes="Bearer"
            services.ConfigureApplicationCookie(config =>
            {
                config.Events = new CookieAuthenticationEvents
                {
                    //OnRedirectToAccessDenied
                    OnRedirectToLogin = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api"))
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        }
                        else
                        {
                            ctx.Response.Redirect("auth/login");
                            //ctx.Response.Redirect(ctx.RedirectUri);
                        }
                        return Task.FromResult(0);
                    }
                };
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;

                if (Environment.IsDevelopment())
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 3;
                }
                else
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 6;

                }

                options.User.RequireUniqueEmail = true;
            });

            services.AddOpenIddict()

                // Register the OpenIddict core services.
                .AddCore(options =>
                {
                    // Register the Entity Framework stores and models.
                    options.UseEntityFrameworkCore()
                        .UseDbContext<ApplicationDbContext>();

                })

                // Register the OpenIddict server handler.
                .AddServer(options =>
                {
                    // Register the ASP.NET Core MVC binder used by OpenIddict.
                    // Note: if you don't call this method, you won't be able to
                    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                    options.UseMvc();

                    // Enable the token endpoint.
                    options.EnableTokenEndpoint("/connect/token");
                    // todo: this should be called on logout
                    options.EnableLogoutEndpoint("/connect/logout");

                    // Enable the password flow.
                    options.AllowPasswordFlow();
                    options.AllowRefreshTokenFlow();
                    options.SetAccessTokenLifetime(TimeSpan.FromSeconds(60 * 10));
                    options.SetRefreshTokenLifetime(TimeSpan.FromDays(1));

                    // Accept anonymous clients (i.e clients that don't send a client_id).
                    options.AcceptAnonymousClients();

                    // During development, you can disable the HTTPS requirement.
                    options.DisableHttpsRequirement();

                    options.RegisterScopes(
                        //OpenIdConnectConstants.Scopes.Email,
                        OpenIdConnectConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Roles
                    );


                    // Note: to use JWT access tokens instead of the default
                    // encrypted format, the following lines are required:
                    //
                    //options.UseJsonWebTokens();
                    //options.AddEphemeralSigningKey();
                })

                // Register the OpenIddict validation handler.
                // Note: the OpenIddict validation handler is only compatible with the
                // default token format or with reference tokens and cannot be used with
                // JWT tokens. For JWT tokens, use the Microsoft JWT bearer handler.
                .AddValidation();

            //services.AddMvc(o => o.EnableEndpointRouting = false); - needed if .UseMvc
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var db = scope.ServiceProvider.GetService<ApplicationDbContext>())
            using (var um = scope.ServiceProvider.GetService<UserManager<User>>())
            using (var rm = scope.ServiceProvider.GetService<RoleManager<AppRole>>())
            {
                // if not using migrations:
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // if using migrations:
                //db.Database.EnsureCreated();
                //db.Database.Migrate();

                SeedAsync(db, um, rm).GetAwaiter().GetResult();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";
                spa.Options.StartupTimeout = new TimeSpan(0, 5, 0);

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }

        private async Task CreateUserAsync(UserManager<User> um, string email, string password, string firstName, string lastName, AppRole role)
        {
            User user = await um.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };
                var result = await um.CreateAsync(user, password);
                if (!result.Succeeded) throw new Exception(string.Join(", ", result.Errors));
            }

            if (!await um.IsInRoleAsync(user, role.Name))
                await um.AddToRoleAsync(user, role.Name);
        }

        private async Task SeedAsync(ApplicationDbContext db, UserManager<User> um, RoleManager<AppRole> rm)
        {
            var role = new AppRole { Name = "Administrator" };
            if (await rm.FindByNameAsync(role.Name) == null) await rm.CreateAsync(role);
            await CreateUserAsync(um, "abc@xyz.com", "ABC@xyz123!", "Abc", "Xyz", role);

            
        }
    }
}
