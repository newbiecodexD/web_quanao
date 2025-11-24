using System;
using System.Collections.Generic;

namespace web_quanao.Core.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Product> Products { get; set; }
    }
}
