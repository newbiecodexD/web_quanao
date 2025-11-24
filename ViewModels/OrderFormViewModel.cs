using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace web_quanao.ViewModels
{
    public class OrderFormViewModel
    {
        public int? OrderId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }
        [Required]
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<OrderItemVm> Items { get; set; } = new List<OrderItemVm>();
    }

    public class OrderItemVm
    {
        public int? OrderItemId { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int SizeId { get; set; }
        [Required, Range(1, 10000)]
        public int Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }
    }
}
