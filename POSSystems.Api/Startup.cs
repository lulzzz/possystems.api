using AutoMapper;
using FluentScheduler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using POSSystems.Core;
using POSSystems.Core.Dtos;
using POSSystems.Core.Dtos.Sales;
using POSSystems.Persistence;
using POSSystems.Web.Infrastructure;
using POSSystems.Web.Infrastructure.Filters;
using POSSystems.Web.Infrastructure.HealthChecks;
using POSSystems.Web.Infrastructure.Jobs;
using POSSystems.Web.Infrastructure.Plugins.DataProviders;
using POSSystems.Web.Infrastructure.Plugins.DataProviders.VSS;
using POSSystems.Web.Infrastructure.Services;
using POSSystems.Web.Infrastructure.Token;
using POSSystems.Web.Infrastructure.TranCloud;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace POSSystems.Web
{
    public class Startup
    {
        private const string SecretKey = "needtogetthisfromenvironment";
        private readonly SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
        private readonly string POSAllowSpecificOrigins = "_posAllowSpecificOrigins";

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            CurrentEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }

        private IWebHostEnvironment CurrentEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (CurrentEnvironment.IsDevelopment())
            {
                services.AddLogging(config =>
                {
                    // clear out default configuration
                    config.ClearProviders();
                    config.AddConsole();
                });
            }

            services.AddCors(options =>
            {
                options.AddPolicy(name: POSAllowSpecificOrigins,
                                  builder =>
                                  {
                                      builder.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
                                  });
            });


            services.AddMemoryCache();

            services.AddOptions();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.ProductVersion;

            services.Configure<ApplicationData>(options =>
            {
                options.Version = version;
            });

            services.AddSingleton(resolver => resolver.GetService<IOptions<ApplicationData>>().Value);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDataProvider, VSSDataProvider>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();

            // Get options from app settings
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
                options.ValidFor = TimeSpan.Parse(jwtAppSettingOptions[nameof(JwtIssuerOptions.ValidFor)]);
                options.DevUser = jwtAppSettingOptions[nameof(JwtIssuerOptions.DevUser)];
                options.DevPass = jwtAppSettingOptions[nameof(JwtIssuerOptions.DevPass)];
            });

            services.AddScoped<CheckPermissionAttribute>();

            //services.AddHttpCacheHeaders(
            //    (expirationModelOptions)
            //    =>
            //    {
            //        expirationModelOptions.MaxAge = 600;
            //    },
            //    (validationModelOptions)
            //    =>
            //    {
            //        validationModelOptions.AddMustRevalidate = true;
            //    });

            services.Configure<TranCloudConfig>(Configuration.GetSection("TranCloud"));
            services.Configure<AzureConfig>(Configuration.GetSection("Azure"));
            services.Configure<JobConfig>(Configuration.GetSection("Job"));
            services.Configure<PrintingInfo>(Configuration.GetSection("PrintingInfo"));
            services.Configure<DeploymentDto>(Configuration.GetSection("Deployment"));

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v3", new OpenApiInfo
                {
                    Title = "POSSystems API",
                    Version = "3.0",
                    Description = "POSSystems API doc"
                });

                var securitySchema = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securitySchema);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    { securitySchema, new[] { "Bearer" } }
                };

                c.AddSecurityRequirement(securityRequirement);

            });

            services.AddSession();

            services.AddMemoryCache();

            services.AddScoped<UserCultureInfo>();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = true,
                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(jwt =>
                {
                    jwt.TokenValidationParameters = tokenValidationParameters;
                });

            if (CurrentEnvironment.IsProduction())
            {
                var healthcheck = services.AddHealthChecks()
                    .AddSqlServer(Configuration["ConnectionStrings:DefaultConnection"], name: "SQL Connection Check");

                if (Configuration.GetSection("Trancloud") != null)
                {
                    var tranuri = new Uri(Configuration["TranCloud:Url"]);
                    healthcheck.AddCheck("Internet", new PingHealthCheck("www.google.com", 500, 300));
                }

                services.AddHealthChecksUI().AddInMemoryStorage();
            }

            services.AddMvc(options => options.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            //app.UseHttpCacheHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (exceptionHandlerFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500, exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);
                        }
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
            }

            app.UseSwagger();
            app.UseSwaggerUI(s =>
            {
                s.SwaggerEndpoint("/swagger/v3/swagger.json", "POSSystems");
            });

            app.UseCors(POSAllowSpecificOrigins);

            //app.UseStaticFiles(new StaticFileOptions()
            //{
            //    FileProvider = new PhysicalFileProvider(
            //        Path.Combine(Directory.GetCurrentDirectory(), @"Static")),
            //    RequestPath = new PathString("/Static")
            //});

            //app.UseHttpCacheHeaders();
            app.UseSession();
            app.UseAuthentication();

            if (env.IsProduction())
            {
                app.UseHealthChecks("/healthz", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                app.UseHealthChecksUI();
            }

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var jobConfig = serviceScope.ServiceProvider.GetService<IOptions<JobConfig>>();
                var unitofwork = new UnitOfWork(Configuration.GetConnectionString("DefaultConnection"));
                var applicationData = serviceScope.ServiceProvider.GetService<ApplicationData>();

                if (!string.IsNullOrEmpty(jobConfig.Value.Sigis))
                    JobManager.Initialize(new SigisRegistry(unitofwork,
                        serviceScope.ServiceProvider.GetService<IOptions<AzureConfig>>(),
                        serviceScope.ServiceProvider.GetService<ILogger<SigisJob>>(),
                        applicationData,
                        serviceScope.ServiceProvider.GetService<IMapper>()));

                if (!string.IsNullOrEmpty(jobConfig.Value.Edi832))
                    JobManager.Initialize(new Edi832Registry(unitofwork, jobConfig,
                        serviceScope.ServiceProvider.GetService<ILogger<Edi832Job>>(),
                        applicationData));

                if (!string.IsNullOrEmpty(jobConfig.Value.Edi855))
                    JobManager.Initialize(new Edi855Registry(unitofwork, jobConfig,
                       serviceScope.ServiceProvider.GetService<ILogger<Edi855Job>>(),
                       applicationData));

                if (!string.IsNullOrEmpty(jobConfig.Value.RetailInsights))
                    JobManager.Initialize(new RetailInsightsRegistry(unitofwork, jobConfig,
                       serviceScope.ServiceProvider.GetService<ILogger<RetailInsightsJob>>(),
                       applicationData));
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });            
        }
    }
}