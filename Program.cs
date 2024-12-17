
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using PersonalWebApi.Entities;
using PersonalWebApi.Seeder;
using PersonalWebApi.Services;
using System.Reflection;

namespace PersonalWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var builder = WebApplication.CreateBuilder(args);

            //// Add services to the container.

            //builder.Services.AddControllers();
            //// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            //builder.Services.AddOpenApi();

            //var app = builder.Build();

            //// Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.MapOpenApi();
            //}

            //app.UseHttpsRedirection();

            //app.UseAuthorization();


            //app.MapControllers();

            //app.Run();


            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<PersonalWebApiDbContext>();

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PersonalWebApi", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            #region add services

            // Register Seeder
            builder.Services.AddScoped<RoleSeeder>();
            builder.Services.AddScoped<UserSeeder>();

            // Configure services for controllers
            builder.Services.AddScoped<IAccountService, AccountService>();

            // configure hash service
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();

        }
    }
}
