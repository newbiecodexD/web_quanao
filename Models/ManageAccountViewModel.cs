using System.ComponentModel.DataAnnotations;

namespace web_quanao.Models
{
    public class ManageAccountViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "??a ch? nh?n hàng")]
        [StringLength(250)]
        public string ShippingAddress { get; set; }
    }
}
