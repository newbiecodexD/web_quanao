using System.Data.Entity;

namespace web_quanao.Persistence.Carts
{
    public class CartDbContext : DbContext
    {
        public CartDbContext() : base("ClothingStoreDb") { }

        public DbSet<CartRecord> Carts { get; set; }
        public DbSet<CartItemRecord> CartItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CartRecord>().ToTable("Carts").HasKey(x => x.CartId);
            modelBuilder.Entity<CartItemRecord>().ToTable("CartItems").HasKey(x => x.CartItemId);

            modelBuilder.Entity<CartRecord>()
                .Property(x => x.Token)
                .HasMaxLength(64)
                .IsUnicode(true);

            modelBuilder.Entity<CartRecord>()
                .Property(x => x.IsAnonymous)
                .IsRequired();

            modelBuilder.Entity<CartItemRecord>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);
        }
    }
}
