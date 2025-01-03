using Insurance.Data;
using Insurance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
                return NotFound();
            }

            // Удаляем текущие роли пользователя
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Добавляем новую роль
            var result = await _userManager.AddToRoleAsync(user, newRole);
            if (result.Succeeded)
            {
                // Обновляем поле Role вручную
                user.Role = newRole;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("ManageUsers");
            }

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
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Не удалось удалить пользователя");
            }

            return RedirectToAction("ManageUsers");
        }
    }
}