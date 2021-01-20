using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WEB.Error;
using WEB.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace WEB
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IWebHostEnvironment Environment { get; }
        private Settings Settings { get; set; }
        private DbContextOptions Options { get; set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers(options => options.Filters.Add(typeof(ApiExceptionAttribute)))
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new DateTimeConverter()));

            var settings = Configuration.GetSection("Settings").Get<Settings>();
            settings.RootPath = Environment.ContentRootPath + (Environment.ContentRootPath.EndsWith(@"\") ? "" : @"\");
            settings.IsDevelopment = Environment.IsDevelopment();
            services.AddSingleton(settings);
            Settings = settings;

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), sqlOptions => sqlOptions.CommandTimeout(300));

                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();

                services.AddSingleton(options.Options);

                // store for seeding
                this.Options = options.Options;
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            // Register the Identity services.
            services.AddIdentity<User, AppRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;

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
                    // todo: in settings
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
                    options.SetLogoutEndpointUris("/connect/logout")
                           .SetTokenEndpointUris("/connect/token");

                    // Enable the flows
                    options.AllowPasswordFlow()
                            .AllowRefreshTokenFlow();

                    options.RegisterScopes(Scopes.Profile, Scopes.Roles);

                    options.SetAccessTokenLifetime(TimeSpan.FromMinutes(settings.AccessTokenExpiryMinutes));
                    options.SetRefreshTokenLifetime(TimeSpan.FromMinutes(settings.RefreshTokenExpiryMinutes));

                    // Register the signing and encryption credentials.
                    if (settings.IsDevelopment && false)
                    {
                        options.AddDevelopmentEncryptionCertificate()
                               .AddDevelopmentSigningCertificate();
                    }
                    else
                    {
                        var certificate = Certificate.GetCertificate(settings);
                        options.AddEncryptionCertificate(certificate);
                        options.AddSigningCertificate(certificate);
                    }

                    // Force client applications to use Proof Key for Code Exchange (PKCE).
                    options.RequireProofKeyForCodeExchange();

                    options.UseAspNetCore()
                        .EnableTokenEndpointPassthrough()
                        //.DisableTransportSecurityRequirement()
                        ;

                    // Note: if you don't want to specify a client_id when sending
                    // a token or revocation request, uncomment the following line:
                    //
                    options.AcceptAnonymousClients();

                    // Note: if you want to process authorization and token requests
                    // that specify non-registered scopes, uncomment the following line:
                    //
                    options.DisableScopeValidation();

                    // Note: if you don't want to use permissions, you can disable
                    // permission enforcement by uncommenting the following lines:
                    //
                    options.IgnoreEndpointPermissions()
                           .IgnoreGrantTypePermissions()
                           .IgnoreResponseTypePermissions()
                           .IgnoreScopePermissions();

                    // Note: when issuing access tokens used by third-party APIs
                    // you don't own, you can disable access token encryption:
                    //
                    // options.DisableAccessTokenEncryption();
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Configure the audience accepted by this resource server.
                    // The value MUST match the audience associated with the
                    // "demo_api" scope, which is used by ResourceController.
                    //options.AddAudiences("resource_server");

                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();

                    // For applications that need immediate access token or authorization
                    // revocation, the database entry of the received tokens and their
                    // associated authorizations can be validated for each API call.
                    // Enabling these options may have a negative impact on performance.
                    //
                    // options.EnableAuthorizationEntryValidation();
                    // options.EnableTokenEntryValidation();
                });

            services.ConfigureApplicationCookie(config =>
            {
                config.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
                config.Events.OnRedirectToLogin = context =>
                {
                    // redirect to /auth/login here?
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };

            });

            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton(Configuration);
            if (!settings.IsDevelopment)
            {
                //services.AddApplicationInsightsTelemetry();
            }

            // allows model state errors to be returned
            services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var db = scope.ServiceProvider.GetService<ApplicationDbContext>())
            using (var um = scope.ServiceProvider.GetService<UserManager<User>>())
            using (var rm = scope.ServiceProvider.GetService<RoleManager<AppRole>>())
            {
                db.InitAsync(um, rm, Settings, this.Options).Wait();
            }

            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next();
            });

            // todo: put this in settings?
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                spa.Options.StartupTimeout = new TimeSpan(0, 5, 0);

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }

        public class DateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return DateTime.Parse(reader.GetString()).ToUniversalTime();
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ"));
            }
        }
    }
}
