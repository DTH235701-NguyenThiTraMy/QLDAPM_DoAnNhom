
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMPetCare.Data;

namespace PMPetCare.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(staffId))
                return Challenge();

            var appointments = _context.Appointments
                .Where(a => a.StaffId == staffId);

            var today = DateTime.Today;

            ViewBag.StaffName = await _context.Users
                .Where(u => u.Id == staffId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync();

            ViewBag.TotalAppointments = await appointments.CountAsync();

            ViewBag.TodayAppointments = await appointments
                .CountAsync(a => a.AppointmentDate >= today &&
                                 a.AppointmentDate < today.AddDays(1));

            ViewBag.InProgressCount = await appointments
                .CountAsync(a => a.Status == "InProgress");

            ViewBag.CompletedCount = await appointments
                .CountAsync(a => a.Status == "Completed");

            var todayList = await appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Where(a => a.AppointmentDate >= today &&
                            a.AppointmentDate < today.AddDays(1) &&
                            a.Status != "Cancelled")
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(todayList);
        }
    }
}
