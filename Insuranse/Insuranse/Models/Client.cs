using System.ComponentModel.DataAnnotations;

namespace Insurance.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "ФИО клиента")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "ИНН клиента")]
        public string TaxId { get; set; }

        [Display(Name = "Телефон")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Электронная почта")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Тип клиента")]
        public string UserType { get; set; } // "Физическое лицо" или "Юридическое лицо"

        // Связь с контрактами
        [Display(Name = "Контракты")]
        public virtual ICollection<Contract> Contracts { get; set; }
    }
}