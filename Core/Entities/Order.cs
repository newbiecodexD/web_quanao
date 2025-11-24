using System;
using System.Collections.Generic;

namespace web_quanao.Core.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
