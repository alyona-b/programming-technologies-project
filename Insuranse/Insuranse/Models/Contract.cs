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
        [Display(Name = "Сумма страховой выплаты")]
        [Range(0, double.MaxValue, ErrorMessage = "Сумма должна быть положительным числом.")]
        public decimal Payout { get; set; }

        [Required]
        [Display(Name = "Статус")]
        public string Status { get; set; } // "Оплачено" или "Не оплачено"

        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        // Связь с клиентом
        //[Required]
        [Display(Name = "Клиент")]
        public int? ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }
    }
}