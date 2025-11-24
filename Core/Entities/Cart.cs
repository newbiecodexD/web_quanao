using System;
using System.Collections.Generic;

namespace web_quanao.Core.Entities
{
    public class Cart
    {
        public int CartId { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; } // added for guest carts
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CartItem> Items { get; set; }
    }
}
