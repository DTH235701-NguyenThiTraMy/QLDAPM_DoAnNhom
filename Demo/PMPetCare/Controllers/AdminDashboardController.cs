
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMPetCare.Data;
using PMPetCare.Models;

namespace PMPetCare.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminDashboardController(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserCount = await _context.Users.CountAsync();

            ViewBag.CustomerCount =
                (await _userManager.GetUsersInRoleAsync("Customer")).Count;

            ViewBag.StaffCount =
                (await _userManager.GetUsersInRoleAsync("Staff")).Count;

            ViewBag.PetCount = await _context.Pets
                .CountAsync(p => !p.IsDeleted);

            ViewBag.ServiceCount = await _context.Services
                .CountAsync(s => s.IsActive);

            ViewBag.AppointmentCount = await _context.Appointments
                .CountAsync();

            ViewBag.PendingCount = await _context.Appointments
                .CountAsync(a => a.Status == "Pending");

            ViewBag.InProgressCount = await _context.Appointments
                .CountAsync(a => a.Status == "InProgress");

            ViewBag.CompletedCount = await _context.Appointments
                .CountAsync(a => a.Status == "Completed");

            var recentAppointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(recentAppointments);
        }
    }
}
