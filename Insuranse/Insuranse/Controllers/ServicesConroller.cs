using Insurance.Data;
using Insurance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServicesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Главная страница со списком услуг
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services.ToListAsync();

            var user = await _userManager.GetUserAsync(User);
            bool isAdminOrAgent = user != null &&
                                  (await _userManager.IsInRoleAsync(user, "Администратор") ||
                                   await _userManager.IsInRoleAsync(user, "Агент"));

            ViewData["IsAdminOrAgent"] = isAdminOrAgent;
            return View(services);
        }

        // Управление услугами
        [Authorize(Roles = "Администратор,Агент")]
        public async Task<IActionResult> Manage()
        {
            var services = await _context.Services.ToListAsync();
            return View(services);
        }

        // Детали услуги
        public async Task<IActionResult> Details(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool isAdminOrAgent = user != null &&
                                  (await _userManager.IsInRoleAsync(user, "Администратор") ||
                                   await _userManager.IsInRoleAsync(user, "Агент"));

            ViewData["IsAdminOrAgent"] = isAdminOrAgent;
            return View(service);
        }

        // Создание услуги
        [Authorize(Roles = "Администратор,Агент")]
        public IActionResult Create()
        {
            ViewBag.Types = GetTypes();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Администратор,Агент")]
        public async Task<IActionResult> Create(Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Manage));
            }

            ViewBag.Types = GetTypes();
            return View(service);
        }

        // Редактирование услуги
        [Authorize(Roles = "Администратор,Агент")]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            ViewBag.Types = GetTypes();
            return View(service);
        }

        [HttpPost]
        [Authorize(Roles = "Администратор,Агент")]
        public async Task<IActionResult> Edit(Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Update(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Manage));
            }

            ViewBag.Types = GetTypes();
            return View(service);
        }

        // Удаление услуги
        // Страница подтверждения удаления
        [Authorize(Roles = "Администратор,Агент")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            return View(service); // Отображаем страницу подтверждения
        }

        // Выполнение удаления услуги
        [HttpPost]
        [Authorize(Roles = "Администратор,Агент")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }


        // Получение списка типов
        private List<string> GetTypes()
        {
            return new List<string>
            {
                "ДМС",
                "Имущество",
                "КАСКО",
                "ОСАГО",
                "Гражданская ответственность",
                "Страхование вкладов",
                "Страхование жизни"
            };
        }
    }
}