using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace web_quanao.ViewModels
{
    public class ProductFormViewModel
    {
        public int? ProductId { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        [AllowHtml]
        public string Description { get; set; }

        [Required, Range(0, 100000000)]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [RegularExpression("^(Nam|Nữ|Nu|Unisex)$", ErrorMessage = "Giới tính không phù hợp ")]
        public string Gender { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SizeStockItem> Sizes { get; set; }
        public IEnumerable<string> ExistingImages { get; set; }
        // New: allow entering image URLs
        public List<string> ImageUrls { get; set; } = new List<string>();

        public class SizeStockItem
        {
            public int SizeId { get; set; }
            public string SizeName { get; set; }
            public bool Selected { get; set; }
            [Range(0, 1000000)]
            public int Stock { get; set; }
        }
    }
}
