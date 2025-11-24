using System.ComponentModel.DataAnnotations;
using web_quanao.Core.Entities;

namespace web_quanao.Models
{
    public class ShoppingCartItem
    {
        [Key]
        public int ShoppingCartItemID { get; set; }
        public string ShoppingCartId { get; set; } // Dùng để nhóm các item của cùng 1 user (có thể dùng Session ID)
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
