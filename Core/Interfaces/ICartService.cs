using web_quanao.Core.Entities;

namespace web_quanao.Core.Interfaces
{
    public interface ICartService
    {
        Cart GetOrCreateCart(string userId, string sessionId);
        Cart GetCart(string userId, string sessionId);
        void AddItem(string userId, string sessionId, int productId, string name, decimal price, int? variantId, int quantity);
        void UpdateQuantity(string userId, string sessionId, int cartItemId, int quantity);
        void RemoveItem(string userId, string sessionId, int cartItemId);
        int GetItemCount(string userId, string sessionId);
        void MergeCart(string fromSessionId, string toUserId);
        decimal CalculateCartTotal(Cart cart);
    }
}
