using System.ComponentModel.DataAnnotations;

namespace web_quanao.ViewModels
{
    public class CategoryFormViewModel
    {
        public int? CategoryId { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }
    }
}
