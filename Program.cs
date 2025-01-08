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
using Microsoft.EntityFrameworkCore;
using PersonalWebApi.Services.Azure;
using Microsoft.SemanticKernel;
using PersonalWebApi.Extensions;
using System.Diagnostics.CodeAnalysis;
using PersonalWebApi.Services.Services.Qdrant;
using PersonalWebApi.Services.Services.Agent;
using PersonalWebApi.Utilities.Utilities.Qdrant;
using PersonalWebApi.Utilities.Utilities.HttUtils;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using PersonalWebApi.Seeder.Agent.History;
using PersonalWebApi.Seeder.Seeder.PageContent;

namespace PersonalWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // AddAsync configuration sources
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.Nlog.json", optional: true, reloadOnChange: true)
                                 //.AddJsonFile("appsettings.SemanticKernel.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.OpenAi.json", optional: true, reloadOnChange: true)
                                 //.AddJsonFile("appsettings.KernelMemory.json", optional: true, reloadOnChange: true)
                                 //.AddJsonFile("appsettings.NlogAzureInsights.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.Qdrant.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.Azure.json", optional: true, reloadOnChange: true)
                                 //.AddJsonFile("semantickernelsettings.json", optional: true, reloadOnChange: true)
                                 .AddUserSecrets<Program>()
                                 .AddEnvironmentVariables();

            // Configure NLog
            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            builder.Host.UseNLog();

            // Load NLog configuration from nlogsettings_azureinsightsapp.json
            //var logger = NLog.LogManager.Setup().LoadConfigurationFromFile("nlogsettings_azureinsightsapp.json").GetCurrentClassLogger();
            var logger = NLog.LogManager.Setup().LoadConfigurationFromFile("nlogsettings.json").GetCurrentClassLogger();

            logger.Debug("init main");

            try
            {
                // AddAsync services to the container
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

                // register semantic kernel services and kernel memory services
                builder
                    .AddSemanticKernelServices()
                    .AddKernelMemoryServices(); // Dodanie Kernel Memory jako us³ugi

                // Register Seeder
                builder.Services.AddScoped<RoleSeeder>();
                builder.Services.AddScoped<UserSeeder>();
                builder.Services.AddScoped<HistoryCosmosDbSeeder>();
                builder.Services.AddScoped<PageContentDbSeeder>();

               

                // Configure services for controllers
                builder.Services.AddScoped<IAccountService, AccountService>();
                builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
                builder.Services.AddScoped<IDocumentReaderDocx, DocumentReaderDocx>();
                builder.Services.AddScoped<IQdrantFileService, QdrantFileService>();
                builder.Services.AddScoped<QdrantRestApiClient>();
                builder.Services.AddScoped<IEmbedding, EmbeddingOpenAi>();

                builder.Services.AddScoped<ICosmosDbService, AzureCosmosDbService>();


                // Register utils
                builder.Services.AddScoped<IApiClient, ApiClient>();

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

                // Apply migrations and run seeders
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<PersonalWebApiDbContext>();
                    dbContext.Database.Migrate();

                    var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
                    roleSeeder.SeedBasic();

                    var userSeeder = scope.ServiceProvider.GetRequiredService<UserSeeder>();
                    userSeeder.SeedBasic();

                    var historyCosmosSeeder = scope.ServiceProvider.GetRequiredService<HistoryCosmosDbSeeder>();
                    historyCosmosSeeder.SeedIfDbAndContainerNotExists();

                    var pageContentCosmosSeeder = scope.ServiceProvider.GetRequiredService<PageContentDbSeeder>();
                    pageContentCosmosSeeder.SeedIfDbAndContainerNotExists();
                }

                //
                // the HTTP request pipeline.
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My PersonalAPI V1");
                    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
                });

                #region register middlewares

                // System middleware
                app.UseMiddleware<ErrorHandlingMiddleware>();

                #endregion

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();

                // Secure Swagger UI
                app.UseWhen(context => context.Request.Path.StartsWithSegments("/swagger"), appBuilder =>
                {
                    appBuilder.UseAuthentication();
                    appBuilder.UseAuthorization();
                    appBuilder.Use(async (context, next) =>
                    {
                        if (!context.User.Identity.IsAuthenticated)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return;
                        }
                        await next();
                    });
                });

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

