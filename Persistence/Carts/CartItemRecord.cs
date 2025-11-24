using System;

namespace web_quanao.Persistence.Carts
{
    public class CartItemRecord
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual CartRecord Cart { get; set; }
    }
}
