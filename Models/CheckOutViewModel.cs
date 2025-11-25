using System.Collections.Generic;

namespace web_quanao.Models
{
    public class CheckOutViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public string DeliveryAddress { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; }
    }
}