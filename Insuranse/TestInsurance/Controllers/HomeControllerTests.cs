using System.Security.Claims;
using System.Threading.Tasks;
using Insurance.Controllers;
using Insurance.Data;
using Insurance.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace TestInsurance.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        [Test]
        public async Task Index_ReturnsViewResult()
        {
            // Мок для ILogger<HomeController>
            var loggerMock = new Mock<ILogger<HomeController>>();

            // Мок для IUserStore<ApplicationUser>
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null
            );

            // Настройка GetUserAsync и GetRolesAsync
            userManagerMock
                .Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { UserName = "TestUser" });

            userManagerMock
                .Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "Администратор" });

            // Настройка тестовой базы данных
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;
            var context = new ApplicationDbContext(options);

            // Добавление тестовых данных
            context.Services.Add(new Service
            {
                Id = 1,
                Name = "Test Service",
                Type = "Test Type",
                Price = 1000,
                Description = "Test Description"
            });
            context.SaveChanges();

            // Создание HomeController
            var controller = new HomeController(loggerMock.Object, userManagerMock.Object, context);

            // Настройка User в тестовом контексте
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, "TestUser"),
    };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<Service>>(result.Model);

            var services = (IEnumerable<Service>)result.Model;
            Assert.AreEqual(1, services.Count());
        }

    }
}
