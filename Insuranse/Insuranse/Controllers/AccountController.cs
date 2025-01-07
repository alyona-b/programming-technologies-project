using Insurance.Data;
using Insurance.Models; // Добавьте этот using для доступа к DashboardViewModel
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Insurance.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Неверные данные");
            return View();
        }

        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Создание объекта ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    TaxId = model.TaxId,
                    PhoneNumber = model.Phone,
                    UserType = model.UserType,
                    Role = "Клиент" // Устанавливаем роль по умолчанию
                };

                // Создание пользователя
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Присвоение роли "Клиент" новому пользователю
                    await _userManager.AddToRoleAsync(user, "Клиент");

                    // Создание записи в таблице Client
                    var client = new Client
                    {
                        Name = model.Name,
                        TaxId = model.TaxId,
                        PhoneNumber = model.Phone,
                        Email = model.Email,
                        UserType = model.UserType
                    };

                    // Сохранение записи клиента в базе данных
                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();

                    // Автоматический вход после регистрации
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Home");
                }

                // Обработка ошибок при создании пользователя
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Если валидация модели не прошла, вернуть форму с ошибками
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Dashboard()
        {
            var userEmail = User.Identity.Name;

            var contracts = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.Agent)
                .Include(c => c.Service)
                .Where(c => c.Client.Email == userEmail || c.Agent.Email == userEmail)
                .ToListAsync();

            // Отфильтровать проблемные контракты
            var problematicContracts = contracts.Where(c => c.Status != "Оплачено" || (c.EndDate - DateTime.Now).Days < 30).ToList();

            var model = new DashboardViewModel
            {
                Contracts = contracts,
                ProblematicContracts = problematicContracts
            };

            return View(model);
        }
    }
}
