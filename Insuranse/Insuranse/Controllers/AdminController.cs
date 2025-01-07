using Insurance.Data;
using Insurance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Insurance.Controllers
{
    [Authorize(Roles = "Администратор")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // Отображение списка пользователей
        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    TaxId = user.TaxId,
                    Role = roles.FirstOrDefault() ?? "Нет роли",
                    UserType = user.UserType,
                    PhoneNumber = user.PhoneNumber
                });
            }

            return View(userList);
        }

        // Изменение роли пользователя
        [HttpPost]
        public async Task<IActionResult> UpdateRole(string userId, string newRole)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newRole))
            {
                return RedirectToAction("ManageUsers");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Пользователь с Id = {userId} не найден.");
            }

            // Удаляем текущие роли пользователя
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Добавляем новую роль
            var result = await _userManager.AddToRoleAsync(user, newRole);
            if (result.Succeeded)
            {
                // Удаляем пользователя из таблицы Clients, если он переводится на роль "Агент" или "Администратор"
                if (newRole == "Агент" || newRole == "Администратор")
                {
                    var client = _context.Clients.FirstOrDefault(c => c.Email == user.Email);
                    if (client != null)
                    {
                        _context.Clients.Remove(client);
                        await _context.SaveChangesAsync();
                    }
                }

                // Если новая роль "Агент", добавляем в таблицу Agents, если данных там ещё нет
                if (newRole == "Агент")
                {
                    var existingAgent = _context.Agents.FirstOrDefault(a => a.Email == user.Email);
                    if (existingAgent == null)
                    {
                        var agent = new Agent
                        {
                            Name = user.Name,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber
                        };
                        _context.Agents.Add(agent);
                        await _context.SaveChangesAsync();
                    }
                }

                // Ручное обновление роли в AspNetUsers
                user.Role = newRole;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("ManageUsers");
            }

            ModelState.AddModelError("", "Не удалось обновить роль пользователя.");
            return RedirectToAction("ManageUsers");
        }

        // Удаление пользователя
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("ManageUsers");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Пользователь с Id = {userId} не найден.");
            }

            // Удаляем агента, если пользователь был агентом
            var agent = _context.Agents.FirstOrDefault(a => a.Email == user.Email);
            if (agent != null)
            {
                _context.Agents.Remove(agent);
                await _context.SaveChangesAsync();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Не удалось удалить пользователя.");
            }

            return RedirectToAction("ManageUsers");
        }
    }
}
