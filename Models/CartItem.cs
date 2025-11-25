using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web_quanao.Models
{
    public class CartItem
    {
        public int idPro { get; set; }
        public string namePro { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public decimal total => price * quantity;
    }
}