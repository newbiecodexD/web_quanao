using System;
using System.Data.Entity;
using System.Linq;
using web_quanao.Persistence.Carts;

namespace web_quanao.Services.CartDb
{
    public class DbCartService
    {
        private readonly CartDbContext _db;
        public DbCartService() : this(new CartDbContext()) { }
        public DbCartService(CartDbContext db) { _db = db; }

        public CartRecord GetOrCreateForUser(int userId)
        {
            var cart = _db.Carts.Include(c=>c.Items).FirstOrDefault(c => c.UserId == userId);
            if (cart != null) return cart;
            cart = new CartRecord { UserId = userId, IsAnonymous = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _db.Carts.Add(cart);
            _db.SaveChanges();
            return cart;
        }

        public CartRecord GetOrCreateForToken(string token)
        {
            var cart = _db.Carts.Include(c=>c.Items).FirstOrDefault(c => c.Token == token && c.IsAnonymous);
            if (cart != null) return cart;
            cart = new CartRecord { Token = token, IsAnonymous = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _db.Carts.Add(cart);
            _db.SaveChanges();
            return cart;
        }

        public void Merge(string token, int userId)
        {
            if (string.IsNullOrEmpty(token)) return;
            var anon = _db.Carts.Include(c=>c.Items).FirstOrDefault(c => c.Token == token && c.IsAnonymous);
            if (anon == null) return;
            var userCart = GetOrCreateForUser(userId);
            foreach (var it in anon.Items.ToList())
            {
                var existing = userCart.Items?.FirstOrDefault(x => x.ProductId == it.ProductId && x.SizeId == it.SizeId);
                if (existing == null)
                {
                    userCart.Items.Add(new CartItemRecord{ ProductId = it.ProductId, SizeId = it.SizeId, Quantity = it.Quantity, Price = it.Price, CreatedAt = DateTime.UtcNow });
                }
                else
                {
                    existing.Quantity += it.Quantity;
                }
            }
            _db.CartItems.RemoveRange(anon.Items);
            _db.Carts.Remove(anon);
            _db.SaveChanges();
        }

        public void AddItem(int userId, string token, int productId, int sizeId, int quantity, decimal price)
        {
            CartRecord cart = userId > 0 ? GetOrCreateForUser(userId) : GetOrCreateForToken(token);
            var ex = cart.Items?.FirstOrDefault(x => x.ProductId == productId && x.SizeId == sizeId);
            if (ex == null)
            {
                cart.Items.Add(new CartItemRecord { ProductId = productId, SizeId = sizeId, Quantity = quantity, Price = price, CreatedAt = DateTime.UtcNow });
            }
            else ex.Quantity += quantity;
            cart.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();
        }

        public void Update(int userId, string token, int cartItemId, int quantity)
        {
            CartItemRecord item = null;
            if (userId > 0)
                item = _db.CartItems.Include(ci=>ci.Cart).FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == userId);
            else
                item = _db.CartItems.Include(ci=>ci.Cart).FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.Cart.Token == token);
            if (item == null) return;
            if (quantity <= 0) _db.CartItems.Remove(item); else item.Quantity = quantity;
            item.Cart.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();
        }

        public void Remove(int userId, string token, int cartItemId)
        {
            CartItemRecord item = null;
            if (userId > 0)
                item = _db.CartItems.Include(ci=>ci.Cart).FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == userId);
            else
                item = _db.CartItems.Include(ci=>ci.Cart).FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.Cart.Token == token);
            if (item == null) return;
            _db.CartItems.Remove(item);
            item.Cart.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();
        }

        public int Count(int userId, string token)
        {
            if (userId > 0) return _db.CartItems.Count(ci => ci.Cart.UserId == userId);
            if (!string.IsNullOrEmpty(token)) return _db.CartItems.Count(ci => ci.Cart.Token == token);
            return 0;
        }
    }
}
