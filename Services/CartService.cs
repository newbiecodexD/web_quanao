using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using web_quanao.Core.Entities;
using web_quanao.Core.Interfaces;

namespace web_quanao.Services
{
    public class CartService : ICartService
    {
        // In-memory stores
        private static readonly ConcurrentDictionary<string, Cart> SessionCarts = new ConcurrentDictionary<string, Cart>();
        private static readonly ConcurrentDictionary<string, Cart> UserCarts = new ConcurrentDictionary<string, Cart>();
        private static int _nextItemId = 1;

        public Cart GetOrCreateCart(string userId, string sessionId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                return UserCarts.GetOrAdd(userId, _ => new Cart { UserId = userId, CreatedDate = DateTime.UtcNow, Items = new List<CartItem>() });
            }
            return SessionCarts.GetOrAdd(sessionId, _ => new Cart { SessionId = sessionId, CreatedDate = DateTime.UtcNow, Items = new List<CartItem>() });
        }

        public Cart GetCart(string userId, string sessionId)
        {
            if (!string.IsNullOrEmpty(userId) && UserCarts.TryGetValue(userId, out var uCart)) return uCart;
            if (SessionCarts.TryGetValue(sessionId, out var sCart)) return sCart;
            return null;
        }

        public void AddItem(string userId, string sessionId, int productId, string name, decimal price, int? variantId, int quantity)
        {
            var cart = GetOrCreateCart(userId, sessionId);
            if (cart.Items == null) cart.Items = new List<CartItem>();
            var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == variantId);
            if (existing == null)
            {
                existing = new CartItem
                {
                    CartItemId = _nextItemId++,
                    ProductId = productId,
                    ProductVariantId = variantId,
                    Quantity = quantity,
                    Product = new Product { ProductId = productId, ProductName = name, Price = price }
                };
                cart.Items.Add(existing);
            }
            else
            {
                existing.Quantity += quantity;
            }
        }

        public void UpdateQuantity(string userId, string sessionId, int cartItemId, int quantity)
        {
            var cart = GetOrCreateCart(userId, sessionId);
            var item = cart.Items.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item == null) return;
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
        }

        public void RemoveItem(string userId, string sessionId, int cartItemId)
        {
            var cart = GetOrCreateCart(userId, sessionId);
            var item = cart.Items.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item != null)
            {
                cart.Items.Remove(item);
            }
        }

        public int GetItemCount(string userId, string sessionId)
        {
            var cart = GetCart(userId, sessionId);
            return cart?.Items?.Sum(i => i.Quantity) ?? 0;
        }

        public void MergeCart(string fromSessionId, string toUserId)
        {
            if (string.IsNullOrEmpty(fromSessionId) || string.IsNullOrEmpty(toUserId)) return;
            if (!SessionCarts.TryGetValue(fromSessionId, out var guestCart)) return;
            var userCart = GetOrCreateCart(toUserId, null);
            foreach (var item in guestCart.Items.ToList())
            {
                var existing = userCart.Items.FirstOrDefault(i => i.ProductId == item.ProductId && i.ProductVariantId == item.ProductVariantId);
                if (existing == null)
                {
                    // clone
                    userCart.Items.Add(new CartItem
                    {
                        CartItemId = _nextItemId++,
                        ProductId = item.ProductId,
                        ProductVariantId = item.ProductVariantId,
                        Quantity = item.Quantity,
                        Product = item.Product
                    });
                }
                else
                {
                    existing.Quantity += item.Quantity;
                }
            }
            SessionCarts.TryRemove(fromSessionId, out _);
        }

        public decimal CalculateCartTotal(Cart cart)
        {
            if (cart?.Items == null) return 0m;
            return cart.Items.Sum(i =>
            {
                var price = i.Product?.DiscountPrice ?? i.Product?.Price ?? 0m;
                return price * i.Quantity;
            });
        }
    }
}
