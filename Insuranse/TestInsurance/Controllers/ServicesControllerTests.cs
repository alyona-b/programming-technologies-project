using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Insurance.Controllers;
using Insurance.Data;
using Insurance.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace TestInsurance.Controllers
{
    [TestFixture]
    public class ServicesControllerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private ApplicationDbContext _context;

        [SetUp]
        public void SetUp()
        {
            // Инициализация UserManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null
            );

            // Инициализация базы данных в памяти
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase_Services")
                .Options;
            _context = new ApplicationDbContext(options);

            // Добавление тестовых данных
            _context.Services.AddRange(new List<Service>
            {
                new Service { Id = 1, Name = "Страхование жизни", Type = "Страхование жизни", Price = 5000, Description = "Описание услуги 1" },
                new Service { Id = 2, Name = "ОСАГО", Type = "ОСАГО", Price = 3000, Description = "Описание услуги 2" }
            });
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithListOfServices()
        {
            // Arrange
            var controller = new ServicesController(_context, _userManagerMock.Object);

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<Service>>(result.Model);

            var services = (IEnumerable<Service>)result.Model;
            Assert.AreEqual(2, services.Count());
        }

        [Test]
        public async Task Details_ReturnsViewResult_WithService()
        {
            // Arrange
            var controller = new ServicesController(_context, _userManagerMock.Object);

            // Act
            var result = await controller.Details(1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Service>(result.Model);

            var service = (Service)result.Model;
            Assert.AreEqual(1, service.Id);
            Assert.AreEqual("Страхование жизни", service.Name);
        }

        [Test]
        public async Task Details_ReturnsNotFound_WhenServiceDoesNotExist()
        {
            // Arrange
            var controller = new ServicesController(_context, _userManagerMock.Object);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Manage_ReturnsViewResult_WithListOfServices()
        {
            // Arrange
            var controller = new ServicesController(_context, _userManagerMock.Object);

            // Настройка ClaimsPrincipal для авторизации
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "Администратор")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await controller.Manage() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<Service>>(result.Model);

            var services = (IEnumerable<Service>)result.Model;
            Assert.AreEqual(2, services.Count());
        }
    }
}
