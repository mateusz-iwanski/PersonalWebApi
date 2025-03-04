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
using Microsoft.SemanticKernel;
using PersonalWebApi.Extensions;
using System.Diagnostics.CodeAnalysis;
using PersonalWebApi.Services.Services.Qdrant;
using PersonalWebApi.Services.Services.Agent;
using PersonalWebApi.Utilities.Utilities.Qdrant;
using PersonalWebApi.Utilities.Utilities.HttUtils;
using PersonalWebApi.Utilities.Utilities.DocumentReaders;
using PersonalWebApi.Seeder.Agent.History;
using OpenTelemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using PersonalWebApi.Utilities.Kql;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using PersonalWebApi.ActionFilters;
using PersonalWebApi.Services.Services.History;
using PersonalWebApi.Utilities.WebScrapper;
using PersonalWebApi.Utilities.WebScrappers;
using PersonalWebApi.Services.WebScrapper;
using PersonalWebApi.Services.FileStorage;
using PersonalWebApi.Services.NoSQLDB;
using PersonalWebApi.Services.Agent;
using nopCommerceApiHub.WebApi;
using PersonalWebApi.Services.NopCommerce;

namespace PersonalWebApi
{
    public class Program
    {
        [Experimental("SKEXP0050")]  // for SemanticKernelTextChunker
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // AddAsync configuration sources
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.Nlog.json", optional: true, reloadOnChange: true)
                                 //.AddJsonFile("appsettings.SemanticKernel.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.OpenAi.json", optional: true, reloadOnChange: true)
                                 //.AddJsonFile("appsettings.KernelMemory.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.Telemetry.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.Qdrant.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.Azure.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.FileStorage.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.StepAgentMappings.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.NopCommerceApi", optional: true, reloadOnChange: true)
                                 //.AddJsonFile("semantickernelsettings.json", optional: true, reloadOnChange: true)
                                 .AddUserSecrets<Program>()
                                 .AddEnvironmentVariables();

            #region telemetry_settings

            https://learn.microsoft.com/en-us/semantic-kernel/concepts/enterprise-readiness/observability/telemetry-with-app-insights?tabs=Powershell&pivots=programming-language-csharp

            var connectionString = builder.Configuration.GetSection("Telemetry:ApplicationInsights:ConnectionString").Value; //"InstrumentationKey=9b62f058-236b-464a-bb45-d6d1bb4bd5b4;IngestionEndpoint=https://polandcentral-0.in.applicationinsights.azure.com/;LiveEndpoint=https://polandcentral.livediagnostics.monitor.azure.com/;ApplicationId=b63f87eb-cde7-44b7-b57d-56e4f4b58aaf";

            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService("TelemetryApplicationInsightsQuickstart");

            // Enable model diagnostics with sensitive data.
            AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

            using var traceProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddSource("Microsoft.SemanticKernel*")
                .AddAzureMonitorTraceExporter(options => options.ConnectionString = connectionString)
                .Build();

            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddMeter("Microsoft.SemanticKernel*")
                .AddAzureMonitorMetricExporter(options => options.ConnectionString = connectionString)
                .Build();

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                // Add OpenTelemetry as a logging provider
                builder.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resourceBuilder);
                    options.AddAzureMonitorLogExporter(options => options.ConnectionString = connectionString);
                    // Format log messages. This is default to false.
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                });
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            });

            builder.Services.AddSingleton(loggerFactory);

            #endregion telemetry_settings


            //// Configure NLog
            //builder.Logging.ClearProviders();
            //builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            //builder.Host.UseNLog();

            //// Load NLog configuration from nlogsettings_azureinsightsapp.json
            ////var logger = NLog.LogManager.Setup().LoadConfigurationFromFile("nlogsettings_azureinsightsapp.json").GetCurrentClassLogger();
            //var logger = NLog.LogManager.Setup().LoadConfigurationFromFile("nlogsettings.json").GetCurrentClassLogger();

            //logger.Debug("init main");

            try
            {
                builder.Services.AddDbContext<PersonalWebApiDbContext>();

                builder.Services.AddControllers();

               

                #region swagger inject + securiy

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

                #endregion swagger

                #region add services

                // register semantic kernel services and kernel memory services
                builder
                    .AddSemanticKernelServices()
                    .AddKernelMemoryServices();

                builder.Services.AddHttpContextAccessor();

                builder.Services.Configure<StolargoPLApiSettings>(builder.Configuration.GetSection("NopCommerceStolargoPLApiSettings"));
                builder.Services.Configure<StolargoPLTokentSettings>(builder.Configuration.GetSection("NopCommerceStolargoPLTokenSettings"));

                builder.Services.AddScoped<CheckConversationAccessFilter>(); // Register the action filter

                // Register Seeder
                builder.Services.AddScoped<RoleSeeder>();
                builder.Services.AddScoped<UserSeeder>();
                builder.Services.AddScoped<HistoryCosmosDbSeeder>();

                // Configure services for controllers
                builder.Services.AddScoped<IAccountService, AccountService>();
                builder.Services.AddScoped<ITextChunker, SemanticKernelTextChunker>();
                builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
                builder.Services.AddScoped<IDocumentReaderDocx, DocumentReaderDocx>();
                builder.Services.AddScoped<IQdrantService, QdrantService>();
                builder.Services.AddScoped<QdrantRestApiClient>();
                builder.Services.AddScoped<IEmbedding, EmbeddingOpenAi>();
                builder.Services.AddScoped<INoSqlDbService, AzureCosmosDbService>();
                builder.Services.AddScoped<KqlApplicationInsightsApi>();
                builder.Services.AddScoped<IPersistentChatHistoryService, PersistentChatHistoryService>();
                builder.Services.AddScoped<IWebScrapperClient, Firecrawl>();
                builder.Services.AddScoped<IWebScrapperService, WebScrapperService>();

                #region nopCommerceApiHub

                // NopCommrce is inject in kernel extension
                // Add options to bind to the configuration instance
                //builder.Services.Configure<StolargoPLApiSettings>(builder.Configuration.GetSection("NopCommerceStolargoPLApiSettings"));
                //builder.Services.Configure<StolargoPLTokentSettings>(builder.Configuration.GetSection("NopCommerceStolargoPLTokenSettings"));



                #endregion

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

                #region seed migration

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

                }

                #endregion seed migration

                #region register middlewares

                // System middleware
                app.UseMiddleware<ErrorHandlingMiddleware>();

                #endregion

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();

                #region swagger setup

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My PersonalAPI V1");
                    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
                });

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

                #endregion

                app.Run();
            }
            catch (Exception ex)
            {
                // NLog: catch setup errors
                //logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}

