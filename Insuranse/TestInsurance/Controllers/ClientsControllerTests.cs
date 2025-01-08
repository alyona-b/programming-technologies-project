using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Insurance.Controllers;
using Insurance.Data;
using Insurance.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace TestInsurance.Controllers
{
    [TestFixture]
    public class ClientsControllerTests
    {
        [Test]
        public async Task Index_ReturnsViewResult_WithListOfClients()
        {
            // Создаем тестовую базу данных с in-memory базой
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase_Clients")
                .Options;

            var context = new ApplicationDbContext(options);

            // Добавляем тестовые данные
            var testClients = new List<Client>
            {
                new Client { Id = 1, Name = "Иван Иванов", TaxId = "1234567890", PhoneNumber = "12345678901", Email = "ivanov@test.com", UserType = "Физическое лицо" },
                new Client { Id = 2, Name = "ООО Ромашка", TaxId = "9876543210", PhoneNumber = "98765432101", Email = "romashka@test.com", UserType = "Юридическое лицо" }
            };

            context.Clients.AddRange(testClients);
            await context.SaveChangesAsync();

            // Создаем экземпляр ClientsController
            var controller = new ClientsController(context);

            // Настройка ClaimsPrincipal для авторизации
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "Администратор") // Роль для прохождения авторизации
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Создаем объект для поиска с фильтром
            var searchModel = new Client { Name = "Иван Иванов" };

            // Act
            var result = await controller.Index(searchModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<Client>>(result.Model);

            var clients = (IEnumerable<Client>)result.Model;
            Assert.AreEqual(1, clients.Count()); // Ожидаем, что вернется 1 клиент (Иван Иванов)
        }
    }
}
