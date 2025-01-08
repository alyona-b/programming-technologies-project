using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Insurance.Models;
using Insurance.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Insurance.Controllers
{
    [Authorize(Roles = "Администратор,Агент")]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Метод для отображения всех клиентов и обработки поиска
        public async Task<IActionResult> Index([FromQuery] Client searchModel)
        {
            var query = _context.Clients.Include(c => c.Contracts).AsQueryable();

            // Применение фильтров на основе введенных значений
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
                query = query.Where(c => c.Name.Contains(searchModel.Name));

            if (!string.IsNullOrWhiteSpace(searchModel.TaxId))
                query = query.Where(c => c.TaxId.Contains(searchModel.TaxId));

            if (!string.IsNullOrWhiteSpace(searchModel.PhoneNumber))
                query = query.Where(c => c.PhoneNumber.Contains(searchModel.PhoneNumber));

            if (!string.IsNullOrWhiteSpace(searchModel.Email))
                query = query.Where(c => c.Email.Contains(searchModel.Email));

            if (!string.IsNullOrWhiteSpace(searchModel.UserType))
                query = query.Where(c => c.UserType.Contains(searchModel.UserType));

            // Выполнение запроса
            var clients = await query.OrderBy(c => c.Name).ToListAsync();

            // Сохранение значений формы в ViewData для их повторного отображения
            ViewData["Name"] = searchModel.Name;
            ViewData["TaxId"] = searchModel.TaxId;
            ViewData["PhoneNumber"] = searchModel.PhoneNumber;
            ViewData["Email"] = searchModel.Email;
            ViewData["UserType"] = searchModel.UserType;

            return View(clients); // Возвращаем те же данные в представление Index.cshtml
        }
    }
}
