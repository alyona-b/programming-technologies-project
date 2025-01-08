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
    public class AccountControllerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private ApplicationDbContext _context;

        [SetUp]
        public void SetUp()
        {
            // Mock для UserManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null
            );

            // Mock для SignInManager
            var signInManagerContext = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object, signInManagerContext.Object, claimsFactory.Object, null, null, null, null
            );

            // Настройка базы данных в памяти
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase_Account")
                .Options;
            _context = new ApplicationDbContext(options);

            // Добавление тестовых данных
            _context.Clients.Add(new Client
            {
                Id = 1,
                Name = "Test Client",
                Email = "testclient@example.com",
                TaxId = "1234567890",
                PhoneNumber = "123-456-7890",
                UserType = "Физическое лицо"
            });

            _context.Contracts.Add(new Contract
            {
                Id = 1,
                ClientId = 1,
                Status = "Оплачено",
                EndDate = DateTime.Now.AddDays(15),
                ContractNumber = "CN12345" // Добавьте это поле
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
        public void Login_ReturnsViewResult()
        {
            // Arrange
            var controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _context);

            // Act
            var result = controller.Login() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task Login_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);


            var controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _context);

            // Act
            var result = await controller.Login("test@example.com", "wrongpassword") as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(controller.ModelState.ContainsKey(""));
        }

        [Test]
        public async Task Register_ValidModel_CreatesUserAndRedirectsToHome()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                Name = "New User",
                TaxId = "9876543210",
                Phone = "123-456-7890",
                UserType = "Физическое лицо"
            };

            _userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Клиент"))
                .ReturnsAsync(IdentityResult.Success); // Здесь возвращаем правильный тип


            var controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _context);

            // Act
            var result = await controller.Register(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Home", result.ControllerName);

            var user = _context.Clients.FirstOrDefault(c => c.Email == model.Email);
            Assert.IsNotNull(user);
        }

        [Test]
        public async Task Logout_RedirectsToHomeIndex()
        {
            // Arrange
            _signInManagerMock
                .Setup(s => s.SignOutAsync())
                .Returns(Task.CompletedTask);

            var controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _context);

            // Act
            var result = await controller.Logout() as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Home", result.ControllerName);
        }

    }
}
