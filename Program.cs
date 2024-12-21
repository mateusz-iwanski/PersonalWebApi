using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using System.Reflection;
using NLog.Web;
using NLog;
using PersonalWebApi.Entities.System;
using PersonalWebApi.Seeder.System;
using PersonalWebApi.Services.System;
using PersonalWebApi.Validations.System;
using PersonalWebApi.Settings.System;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using PersonalWebApi.Middleware;

namespace PersonalWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add configuration sources
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("nlogsettings.json", optional: true, reloadOnChange: true)
                                 .AddUserSecrets<Program>()
                                 .AddEnvironmentVariables();

            // Configure NLog
            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            builder.Host.UseNLog();

            // Load NLog configuration from nlogsettings.json
            var logger = NLog.LogManager.Setup().LoadConfigurationFromFile("nlogsettings.json").GetCurrentClassLogger();
            
            logger.Debug("init main");

            try
            {
                // Add services to the container
                builder.Services.AddDbContext<PersonalWebApiDbContext>();

                builder.Services.AddControllers();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "PersonalAPI", Version = "v1" });

                    // Set the comments path for the Swagger JSON and UI.
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    options.IncludeXmlComments(xmlPath);

                    // Define the Bearer security scheme
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Get token from /api/system/account/login and write like this: \"Bearer copied_token\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                    });

                    // Use the Bearer scheme globally
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                BearerFormat = "JWT",
                                In = ParameterLocation.Header,
                            },
                            new List<string>()
                        }
                    });
                });

                #region add services

                // Register Seeder
                builder.Services.AddScoped<RoleSeeder>();
                builder.Services.AddScoped<UserSeeder>();

                // Configure services for controllers
                builder.Services.AddScoped<IAccountService, AccountService>();

                // Configure password hasher
                builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

                // Authentication settings
                var authenticationSettings = new AuthenticationSettings();
                builder.Configuration.GetSection("Authentication").Bind(authenticationSettings);
                builder.Services.AddSingleton(authenticationSettings);

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Bearer";
                    options.DefaultChallengeScheme = "Bearer";
                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = authenticationSettings.JwtIssuer,
                        ValidAudience = authenticationSettings.JwtIssuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey))
                    };
                });

                #endregion

                #region add Middlewares

                // System middleware
                builder.Services.AddScoped<ErrorHandlingMiddleware>();

                #endregion

                #region add custom validator

                builder.Services.AddFluentValidationAutoValidation()
                                .AddFluentValidationClientsideAdapters();
                builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserDtoValidator>();
                builder.Services.AddValidatorsFromAssemblyContaining<RoleCreateDtoValidator>();

                #endregion

                var app = builder.Build();

                // Run seeders
                using var scope = app.Services.CreateScope();

                var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
                roleSeeder.SeedBasic();
                var userSeeder = scope.ServiceProvider.GetRequiredService<UserSeeder>();
                userSeeder.SeedBasic();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My PersonalAPI V1");
                        c.RoutePrefix = "swagger";
                    });
                }

                #region register middlewares

                // System middleware
                app.UseMiddleware<ErrorHandlingMiddleware>();

                #endregion

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                // NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}
