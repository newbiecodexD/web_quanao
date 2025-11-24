using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace web_quanao.Models
{
    public class Brand
    {
        [Key]
        public int BrandID { get; set; }

        [Required(ErrorMessage = "Tên thương hiệu là bắt buộc")]
        [StringLength(100)]
        public string Name { get; set; }

        // Navigation - EF Database First models
        public virtual ICollection<Product> Products { get; set; }

        public Brand()
        {
            Products = new HashSet<Product>();
        }
    }
}