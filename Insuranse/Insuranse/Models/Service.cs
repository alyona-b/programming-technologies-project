using System.ComponentModel.DataAnnotations;

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

        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }
}