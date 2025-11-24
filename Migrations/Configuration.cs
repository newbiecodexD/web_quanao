using System.Data.Entity.Migrations;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using web_quanao.Infrastructure.Data;

namespace web_quanao.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            // Disable automatic migrations to avoid conflicts with existing tables
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            // Seed roles
            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);
            if (!roleManager.RoleExists("User"))
            {
                roleManager.Create(new IdentityRole("User"));
            }

            // Seed users
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // 1) minh@gmail.com / Minh123@@
            var email1 = "minh@gmail.com";
            if (!context.Users.Any(u => u.UserName == email1))
            {
                var u1 = new ApplicationUser
                {
                    UserName = email1,
                    Email = email1,
                    EmailConfirmed = true,
                    IsActive = true
                };
                var r1 = userManager.Create(u1, "Minh123@@");
                if (r1.Succeeded)
                {
                    userManager.AddToRole(u1.Id, "User");
                }
            }

            // 2) minh123@gmail.com / Minh456@@
            var email2 = "minh123@gmail.com";
            if (!context.Users.Any(u => u.UserName == email2))
            {
                var u2 = new ApplicationUser
                {
                    UserName = email2,
                    Email = email2,
                    EmailConfirmed = true,
                    IsActive = true
                };
                var r2 = userManager.Create(u2, "Minh456@@");
                if (r2.Succeeded)
                {
                    userManager.AddToRole(u2.Id, "User");
                }
            }
        }
    }
}
