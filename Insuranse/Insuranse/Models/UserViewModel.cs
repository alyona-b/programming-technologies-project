namespace Insurance.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string TaxId { get; set; }
        public string Role { get; set; }
        public string UserType { get; set; } // Добавлено: Тип пользователя
        public string PhoneNumber { get; set; } // Добавлено: Номер телефона
    }
}