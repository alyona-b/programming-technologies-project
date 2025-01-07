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
                .Include(c => c.Client) // Жадная загрузка клиента
                .Include(c => c.Agent) // Жадная загрузка агента
                .ToListAsync();

            return View(contracts);
        }


        public IActionResult Create()
        {
            LoadStatusDropdown();
            LoadClientsDropdown();
            LoadAgentsDropdown(); // Загрузка списка агентов

            return View(new Contract());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract)
        {
            // Проверка на валидность ModelState
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid. Details:");

                // Логируем все ошибки из ModelState
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    if (errors.Count > 0)
                    {
                        Console.WriteLine($"Key: {key}");
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"  Error: {error.ErrorMessage}");
                        }
                    }
                }

                Console.WriteLine("Проверьте ошибки выше.");

                // Загружаем данные для повторного отображения формы
                LoadStatusDropdown();
                LoadClientsDropdown();
                LoadAgentsDropdown();
                return View(contract);
            }

            // Проверка, указан ли клиент
            if (contract.ClientId == 0)
            {
                ModelState.AddModelError("ClientId", "Клиент обязателен.");
                Console.WriteLine("Ошибка: ClientId не указан.");
                LoadStatusDropdown();
                LoadClientsDropdown();
                LoadAgentsDropdown();
                return View(contract);
            }

            // Проверка существования клиента в базе данных
            contract.Client = await _context.Clients.FindAsync(contract.ClientId);
            if (contract.Client == null)
            {
                ModelState.AddModelError("ClientId", "Клиент не найден.");
                Console.WriteLine($"Ошибка: Клиент с ID {contract.ClientId} не найден в базе данных.");
                LoadStatusDropdown();
                LoadClientsDropdown();
                LoadAgentsDropdown();
                return View(contract);
            }

            // Проверка, указан ли агент
            if (contract.AgentId == 0)
            {
                ModelState.AddModelError("AgentId", "Агент обязателен.");
                Console.WriteLine("Ошибка: AgentId не указан.");
                LoadStatusDropdown();
                LoadClientsDropdown();
                LoadAgentsDropdown();
                return View(contract);
            }

            // Проверка существования агента в базе данных
            contract.Agent = await _context.Agents.FindAsync(contract.AgentId);
            if (contract.Agent == null)
            {
                ModelState.AddModelError("AgentId", "Агент не найден.");
                Console.WriteLine($"Ошибка: Агент с ID {contract.AgentId} не найден в базе данных.");
                LoadStatusDropdown();
                LoadClientsDropdown();
                LoadAgentsDropdown();
                return View(contract);
            }

            try
            {
                // Добавление контракта в базу данных
                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();
                Console.WriteLine("Договор успешно создан.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Логируем ошибку сохранения в базу данных
                Console.WriteLine($"Ошибка при сохранении контракта: {ex.Message}");
                ModelState.AddModelError("", "Произошла ошибка при сохранении контракта. Попробуйте снова.");
                LoadStatusDropdown();
                LoadClientsDropdown();
                LoadAgentsDropdown();
                return View(contract);
            }
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
            var contract = await _context.Contracts
                .Include(c => c.Client) // Жадная загрузка клиента
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

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
            var contract = await _context.Contracts
                .Include(c => c.Client) // Жадная загрузка клиента
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

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

            if (clients == null || !clients.Any())
            {
                clients.Add(new SelectListItem { Value = "", Text = "Нет доступных клиентов" });
            }

            ViewData["Clients"] = clients;

            // Отладочный вывод
            Console.WriteLine($"Loaded clients count: {clients.Count}");
            foreach (var client in clients)
            {
                Console.WriteLine($"Client: {client.Text}, Id: {client.Value}");
            }

        }

        private void LoadAgentsDropdown()
        {
            var agents = _context.Agents
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
                .ToList();

            if (agents == null || !agents.Any())
            {
                agents.Add(new SelectListItem { Value = "", Text = "Нет доступных агентов" });
            }

            ViewData["Agents"] = agents;

            // Отладочный вывод
            Console.WriteLine($"Loaded agents count: {agents.Count}");
            foreach (var agent in agents)
            {
                Console.WriteLine($"Agent: {agent.Text}, Id: {agent.Value}");
            }
        }

    }
}