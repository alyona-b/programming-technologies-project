using System.ComponentModel.DataAnnotations;

namespace Insurance.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringLength(10)]
        public string TaxId { get; set; }

        [Required, Phone]
        [StringLength(11)]
        public string Phone { get; set; }

        [Required]
        public string UserType { get; set; }
    }
}