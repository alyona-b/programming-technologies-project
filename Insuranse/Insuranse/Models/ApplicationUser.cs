using Microsoft.AspNetCore.Identity;

namespace Insurance.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string TaxId { get; set; }
        public string UserType { get; set; } // "Физическое лицо" или "Юридическое лицо"

        public string Role { get; set; } // Хранение роли пользователя ("Администратор", "Агент"
    }
}