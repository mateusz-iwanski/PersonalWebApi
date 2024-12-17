using PersonalWebApi.Entities.System;

namespace PersonalWebApi.Seeder.System
{
    public class RoleSeeder
    {
        private readonly PersonalWebApiDbContext _context;

        public RoleSeeder(PersonalWebApiDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Check if basic customer roles exist in the database and add them if they don't
        /// </summary>
        public void SeedBasic()
        {
            if (_context.Roles.Count() == 0)
            {

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                if (_context.Roles.FirstOrDefault(a => a.Name == "Administrator") == null)
                    _context.Roles.Add(
                        new Role
                        {
                            Name = "Administrator"
                        }
                    );

                if (_context.Roles.FirstOrDefault(a => a.Name == "User") == null)
                    _context.Roles.Add(
                        new Role
                        {
                            Name = "User"
                        }
                    );

                _context.SaveChanges();
            }
        }
    }
}
