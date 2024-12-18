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
            // Configure NLog
            var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
            logger.Debug("init main");

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add NLog to ASP.NET Core
                builder.Logging.ClearProviders();
                builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.Host.UseNLog();

                builder.Services.AddDbContext<PersonalWebApiDbContext>();

                // Add services to the container.
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
                                BearerFormat = "JWT", // Set the default bearer format to JWT
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

                // configure hash service
                builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

                // Authentication settings
                var authenticationSettings = new AuthenticationSettings();
                /// from appsettings.json
                ConfigurationBinder.Bind(builder.Configuration.GetSection("Authentication"), authenticationSettings);
                builder.Services.AddSingleton(authenticationSettings);
                builder.Services.AddAuthentication(option =>
                {
                    option.DefaultAuthenticateScheme = "Bearer";
                    option.DefaultChallengeScheme = "Bearer";
                    option.DefaultChallengeScheme = "Bearer";
                }).AddJwtBearer(cfg =>
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

                //builder.Services.AddAuthorization(options =>
                //{
                //    options.AddPolicy("Administrator", policy => policy.RequireRole("Administrator"));
                //});

                #endregion

                #region add Middlewares

                // System middleware
                builder.Services.AddScoped<ErrorHandlingMiddleware>();

                #endregion

                #region add custom validator

                builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<RegisterUserDtoValidator>());
                builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<RoleCreateDtoValidator>());

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
