using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web_quanao.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        // Nam | Nu | Unisex
        public string Gender { get; set; }
    }
}