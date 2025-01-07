using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insurance.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Тип")]
        public string Type { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        [Display(Name = "Стоимость")]
        [Range(0, double.MaxValue, ErrorMessage = "Стоимость должна быть положительным числом.")]
        public decimal Price { get; set; }

        [Column(TypeName = "LONGTEXT")]
        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        [StringLength(int.MaxValue, ErrorMessage = "Описание не может превышать {1} символов.")]
        public string Description { get; set; }

    }
}