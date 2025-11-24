using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using web_quanao.Core.Entities;

namespace web_quanao.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        static ApplicationDbContext()
        {
            // Migrations will be run explicitly via Update-Database
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public ApplicationDbContext() : base("UniqloDBContext") { }

        public static ApplicationDbContext Create() => new ApplicationDbContext();

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        // Removed Cart / CartItem DbSets (now in-memory only)

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Ignore in-memory cart entities so EF does not try to migrate them
            modelBuilder.Ignore<Cart>();
            modelBuilder.Ignore<CartItem>();
        }
    }
}
