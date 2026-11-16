using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CampusRooms.Controllers
{
    
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        
        private readonly UserManager<IdentityUser> _user_manager;

        public UsersController(UserManager<IdentityUser> user_manager)
        {
            _user_manager = user_manager;
        }

        [HttpGet("users")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Users";
            List<IdentityUser> users = _user_manager.Users.OrderBy(user => user.Email).ToList();
            IList<IdentityUser> staff = await _user_manager.GetUsersInRoleAsync("Staff");
            ViewBag.StaffIds = staff.Select(user => user.Id).ToList();
            return View(users);
        }

        
        [HttpPost("users/make-staff/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeStaff(string id)
        {
            IdentityUser? user = await _user_manager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            await _user_manager.AddToRoleAsync(user, "Staff");   
            TempData["Message"] = $"{user.Email} is Staff now. They need to log out and in again.";
            return RedirectToAction("Index");
        }
    }
}