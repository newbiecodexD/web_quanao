using System;
using System.ComponentModel.DataAnnotations;

namespace web_quanao.ViewModels
{
    public class UserFormViewModel
    {
        public int? UserId { get; set; }

        [Required, StringLength(200)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("^(Admin|Customer)$", ErrorMessage = "Vai trò không hợp lí ")] 
        public string Role { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }
    }
}
