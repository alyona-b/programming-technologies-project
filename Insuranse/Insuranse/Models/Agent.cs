using System.ComponentModel.DataAnnotations;

namespace Insurance.Models
{
    public class Agent
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "ФИО агента")]
        public string Name { get; set; }

        [Display(Name = "Телефон")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Электронная почта")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        // Связь с контрактами
        [Display(Name = "Договора")]
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
