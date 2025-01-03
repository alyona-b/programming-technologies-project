using Insurance.Data;
using Insurance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Insurance.Controllers
{
    [Authorize(Roles = "Администратор,Агент")]
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContractsController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IActionResult> Index()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Client) // Загрузка связанного объекта Client
                .ToListAsync();

            return View(contracts);
        }


        public IActionResult Create()
        {
            LoadStatusDropdown();
            LoadClientsDropdown();

            var model = new Contract();  // Инициализация модели перед отправкой в представление
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (contract.ClientId.HasValue)
                    {
                        contract.Client = await _context.Clients.FindAsync(contract.ClientId.Value);
                        if (contract.Client == null)
                        {
                            ModelState.AddModelError("ClientId", "Выбранный клиент не существует.");
                            LoadStatusDropdown();
                            LoadClientsDropdown();
                            return View(contract);
                        }
                    }

                    _context.Contracts.Add(contract);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка сохранения: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Произошла ошибка при сохранении контракта.");
                }
            }
            else
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage)).ToList();
                Console.WriteLine("Ошибки валидации: " + string.Join(", ", errors));
            }

            LoadStatusDropdown();
            LoadClientsDropdown();
            return View(contract);
        }



        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            LoadStatusDropdown();
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Contract contract)
        {
            if (ModelState.IsValid)
            {
                _context.Contracts.Update(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadStatusDropdown();
            return View(contract);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            return View(contract);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            return View(contract);
        }

        private void LoadStatusDropdown()
        {
            ViewData["Statuses"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "Оплачено", Text = "Оплачено" },
                new SelectListItem { Value = "Не оплачено", Text = "Не оплачено" }
            };
        }

        private void LoadClientsDropdown()
        {
            var clients = _context.Clients
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            // Если клиентов нет, создайте пустой список
            if (clients == null || !clients.Any())
            {
                clients.Add(new SelectListItem { Value = "", Text = "Нет доступных клиентов" });
            }

            ViewData["Clients"] = clients;
        }

    }
}