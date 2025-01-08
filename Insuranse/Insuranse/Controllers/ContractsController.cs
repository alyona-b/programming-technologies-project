using Insurance.Data;
using Insurance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Insurance.Controllers
{
    //[Authorize(Roles = "Администратор,Агент")]
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContractsController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [Authorize(Roles = "Администратор,Агент")]
        public async Task<IActionResult> Index()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Client)  // Жадная загрузка клиента
                .Include(c => c.Agent)   // Жадная загрузка агента
                .Include(c => c.Service) // Жадная загрузка услуги
                .OrderBy(c => c.ContractNumber) // Сортировка по номеру договора
                .ToListAsync();

            return View(contracts);
        }

        [Authorize(Roles = "Администратор,Агент")]
        public IActionResult Create()
        {
            var model = new Contract
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1)
            };

            LoadStatusDropdown();
            LoadClientsDropdown();
            LoadAgentsDropdown();
            LoadServicesDropdown();
            return View(model);
        }

        [Authorize(Roles = "Администратор,Агент")]

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
                LoadServicesDropdown();
                return View(contract);
            }

            // Проверка дат начала и окончания
            if (contract.StartDate == null || contract.EndDate == null)
            {
                ModelState.AddModelError("StartDate", "Дата начала обязательна.");
                ModelState.AddModelError("EndDate", "Дата окончания обязательна.");
                LoadStatusDropdown();
                LoadClientsDropdown();
                LoadAgentsDropdown();
                LoadServicesDropdown();
                return View(contract);
            }

            if (contract.StartDate > contract.EndDate)
            {
                ModelState.AddModelError("EndDate", "Дата окончания не может быть раньше даты начала.");
                LoadStatusDropdown();
                LoadClientsDropdown();
                LoadAgentsDropdown();
                LoadServicesDropdown();
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
                LoadServicesDropdown();
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
                LoadServicesDropdown();
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
                LoadServicesDropdown();
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
                LoadServicesDropdown();
                return View(contract);
            }

            // Проверка, указана ли услуга
            if (contract.ServiceId == 0)
            {
                ModelState.AddModelError("ServiceId", "Услуга обязательна.");
                Console.WriteLine("Ошибка: ServiceId не указан.");
                LoadStatusDropdown();
                LoadClientsDropdown();
                LoadAgentsDropdown();
                LoadServicesDropdown();
                return View(contract);
            }

            // Проверка существования услуги в базе данных
            contract.Service = await _context.Services.FindAsync(contract.ServiceId);
            if (contract.Service == null)
            {
                ModelState.AddModelError("ServiceId", "Услуга не найдена.");
                Console.WriteLine($"Ошибка: Услуга с ID {contract.ServiceId} не найдена в базе данных.");
                LoadStatusDropdown();
                LoadClientsDropdown();
                LoadAgentsDropdown();
                LoadServicesDropdown();
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
                LoadServicesDropdown();
                return View(contract);
            }
        }

        [Authorize(Roles = "Администратор,Агент")]
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _context.Contracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            LoadStatusDropdown();
            LoadClientsDropdown();
            LoadAgentsDropdown();
            LoadServicesDropdown();
            return View(contract);
        }

        [Authorize(Roles = "Администратор,Агент")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Contract contract)
        {
            if (!ModelState.IsValid)
            {
                LoadStatusDropdown();
                LoadClientsDropdown();
                LoadAgentsDropdown();
                LoadServicesDropdown();
                return View(contract);
            }

            if (contract.StartDate > contract.EndDate)
            {
                ModelState.AddModelError("EndDate", "Дата окончания не может быть раньше даты начала.");
                LoadStatusDropdown();
                LoadClientsDropdown();
                LoadAgentsDropdown();
                LoadServicesDropdown();
                return View(contract);
            }

            try
            {
                _context.Contracts.Update(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Contracts.Any(e => e.Id == contract.Id))
                {
                    return NotFound();
                }

                ModelState.AddModelError("", "Ошибка сохранения изменений. Попробуйте снова.");
            }

            LoadStatusDropdown();
            LoadClientsDropdown();
            LoadAgentsDropdown();
            LoadServicesDropdown();
            return View(contract);
        }

        [Authorize(Roles = "Администратор,Агент")]
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

        [Authorize(Roles = "Администратор,Агент")]
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

        [Authorize(Roles = "Клиент, Агент, Администратор")]
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.Agent)
                .Include(c => c.Service)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            // Проверяем, что клиент имеет доступ к этому договору
            var userEmail = User.Identity.Name;
            if (contract.Client.Email != userEmail && contract.Agent.Email != userEmail)
            {
                return Forbid();  // Запрещаем доступ, если клиент или агент не совпадают с пользователем
            }

            return View(contract);
        }

        [Authorize(Roles = "Администратор,Агент")]
        public async Task<IActionResult> Search([FromQuery] Contract searchModel)
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.Agent)
                .Include(c => c.Service)
                .AsQueryable();

            // Применение фильтров
            if (!string.IsNullOrWhiteSpace(searchModel.ContractNumber))
                query = query.Where(c => c.ContractNumber.Contains(searchModel.ContractNumber));

            if (searchModel.ServiceId > 0)
                query = query.Where(c => c.ServiceId == searchModel.ServiceId);

            if (searchModel.ClientId > 0)
                query = query.Where(c => c.ClientId == searchModel.ClientId);

            if (searchModel.AgentId > 0)
                query = query.Where(c => c.AgentId == searchModel.AgentId);

            if (searchModel.Payout > 0)
                query = query.Where(c => c.Payout == searchModel.Payout);

            if (!string.IsNullOrWhiteSpace(searchModel.Status))
                query = query.Where(c => c.Status == searchModel.Status);

            if (searchModel.StartDate != default)
                query = query.Where(c => c.StartDate == searchModel.StartDate);

            if (searchModel.EndDate != default)
                query = query.Where(c => c.EndDate == searchModel.EndDate);

            // Сортировка по номеру договора в порядке возрастания
            var contracts = await query.OrderBy(c => c.ContractNumber).ToListAsync();

            // Передача данных для сохранения значений формы
            ViewData["ContractNumber"] = searchModel.ContractNumber;
            ViewData["ServiceId"] = searchModel.ServiceId;
            ViewData["ClientId"] = searchModel.ClientId;
            ViewData["AgentId"] = searchModel.AgentId;
            ViewData["Status"] = searchModel.Status;
            ViewData["StartDate"] = searchModel.StartDate != default ? searchModel.StartDate.ToString("yyyy-MM-dd") : "";
            ViewData["EndDate"] = searchModel.EndDate != default ? searchModel.EndDate.ToString("yyyy-MM-dd") : "";
            ViewData["Payout"] = searchModel.Payout;

            // Передача списков для выбора
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name");
            ViewBag.Clients = new SelectList(_context.Clients, "Id", "Name");
            ViewBag.Agents = new SelectList(_context.Agents, "Id", "Name");
            ViewBag.Statuses = new SelectList(new List<SelectListItem>
    {
        new SelectListItem { Value = "Оплачено", Text = "Оплачено" },
        new SelectListItem { Value = "Не оплачено", Text = "Не оплачено" }
    }, "Value", "Text");

            return View("Search", contracts);
        }

        [Authorize(Roles = "Администратор,Агент")]
        private void LoadStatusDropdown()
        {
            ViewData["Statuses"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "Оплачено", Text = "Оплачено" },
                new SelectListItem { Value = "Не оплачено", Text = "Не оплачено" }
            };
        }

        [Authorize(Roles = "Администратор,Агент")]
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

        [Authorize(Roles = "Администратор,Агент")]
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

        [Authorize(Roles = "Администратор,Агент")]
        private void LoadServicesDropdown()
        {
            var services = _context.Services
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToList();

            if (!services.Any())
            {
                services.Add(new SelectListItem { Value = "", Text = "Нет доступных услуг" });
            }

            ViewData["Services"] = services;
        }


    }
}