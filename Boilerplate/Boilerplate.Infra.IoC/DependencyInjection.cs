using Boilerplate.Application.Interfaces;
using Boilerplate.Application.Interfaces.ICurrentUserContext;
using Boilerplate.Application.JobExecutors;
using Boilerplate.Application.JobScheduler;
using Boilerplate.Application.Services;
using Boilerplate.Application.Services.Auth;
using Boilerplate.Application.Services.Email;
using Boilerplate.Application.Services.Tenants;
using Boilerplate.Application.Utils.CurrentUserContext;
using Boilerplate.Domain.Interfaces.Authenticate;
using Boilerplate.Domain.Interfaces.JobExecutors;
using Boilerplate.Domain.Interfaces.Repositories;
using Boilerplate.Domain.Interfaces.Repositories.IUnitOfWork;
using Boilerplate.Infra.Data.Context;
using Boilerplate.Infra.Data.Context.Seeding;
using Boilerplate.Infra.Data.Identity;
using Boilerplate.Infra.Data.Identity.AuthenticateService;
using Boilerplate.Infra.Data.Repositories;
using Boilerplate.Infra.Data.Repositories.UnitOfWork;
using Boilerplate.JobServer.Wrappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Boilerplate.Infra.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfraIoC(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BoilerplateDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<BoilerplateDbContext>()
            .AddDefaultTokenProviders();

            var jwtSettings = configuration.GetSection("JWT");
            var secretKey = jwtSettings["SecretKey"] ?? "your-secret-key-here-make-it-long-enough";

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/systemNotification"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddHttpContextAccessor();

            //Application Services
            services.AddScoped<IEntity1Service, Entity1Service>();
            services.AddScoped<IAuthenticateService, AuthenticateService>();
            services.AddScoped<AuthAppService>();
            services.AddScoped<SeedData>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<ICurrentUserContext, CurrentUserContext>();

            //Hangfire Job Scheduler Wrappers
            services.AddScoped<IEntity1JobScheduler, Entity1Wrapper>();
            services.AddScoped<IEntity1JobExecutor, Entity1Executor>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
