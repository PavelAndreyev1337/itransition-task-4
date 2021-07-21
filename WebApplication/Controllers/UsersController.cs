using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly AccountController _accountController;

        public UsersController(UserManager<User> userManager, AccountController accountController)
        {
            _userManager = userManager;
            _accountController = accountController;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(_userManager.Users.ToList());
        }

        private async Task<IActionResult> ChangeUsers(string[] usersIds, Func<User, Task> handler)
        {
            if (usersIds != null)
            {
                var currentUserExist = false;
                foreach (var userId in usersIds)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await handler(user);
                        var changedUser = await _userManager.FindByIdAsync(user.Id);
                        if (User.Identity.Name == user.UserName && (user.LockoutEnabled || changedUser == null))
                        {
                            currentUserExist = true;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "User not found");
                    }
                }
                if (currentUserExist)
                {
                    return await _accountController.Logout();
                }
            }
            return RedirectToAction("Index", "Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Block(string[] usersIds)
        {
            return await ChangeUsers(usersIds, async user => {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(3);
                await _userManager.UpdateAsync(user);
                await _userManager.UpdateSecurityStampAsync(user);
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unblock(string[] usersIds)
        {
            return await ChangeUsers(usersIds, async user => 
            {
                user.LockoutEnabled = false;
                user.LockoutEnd = DateTimeOffset.UtcNow;
                await _userManager.UpdateAsync(user);
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string[] usersIds)
        {
            return await ChangeUsers(usersIds, async user =>
            {
                await _userManager.DeleteAsync(user);
            });
        }
    }
}
