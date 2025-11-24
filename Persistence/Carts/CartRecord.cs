using System;
using System.Collections.Generic;

namespace web_quanao.Persistence.Carts
{
    public class CartRecord
    {
        public int CartId { get; set; }
        public int? UserId { get; set; } // link to dbo.Users.UserId (EF DB-first uses int)
        public string Token { get; set; } // GUID for anonymous cart, stored in cookie
        public bool IsAnonymous { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<CartItemRecord> Items { get; set; }
    }
}
