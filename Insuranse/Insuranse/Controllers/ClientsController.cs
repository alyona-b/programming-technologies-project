using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Insurance.Models;
using Insurance.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Insurance.Controllers
{
    [Authorize(Roles = "Администратор,Агент")]  // Ограничиваем доступ только для админов и агентов
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Получение списка всех клиентов из таблицы Clients
            var clients = await _context.Clients.ToListAsync();
            return View(clients);
        }



    }
}
