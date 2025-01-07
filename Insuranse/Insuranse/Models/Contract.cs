using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insurance.Models
{
    public class Contract
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Номер договора")]
        public string ContractNumber { get; set; }

        [Required]
        [ForeignKey("Service")]
        [Display(Name = "Услуга")]
        public int ServiceId { get; set; }
        public virtual Service? Service { get; set; }

        [Required]
        [Display(Name = "Сумма страховой выплаты")]
        [Range(0, double.MaxValue, ErrorMessage = "Сумма должна быть положительным числом.")]
        public decimal Payout { get; set; }

        [Required]
        [Display(Name = "Статус")]
        public string Status { get; set; } // "Оплачено" или "Не оплачено"

        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        // Связь с клиентом
        [Required] // Убедитесь, что связь обязательна
        [ForeignKey("Client")]
        public int ClientId { get; set; }

        public virtual Client? Client { get; set; } // Навигационное свойство

        // Связь с агентом
        [Required]
        [ForeignKey("Agent")]
        public int AgentId { get; set; }
        public virtual Agent? Agent { get; set; }

        [Required]
        [Display(Name = "Дата заключения")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Дата окончания")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

    }
}