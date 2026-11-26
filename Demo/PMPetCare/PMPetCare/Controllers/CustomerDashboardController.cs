
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMPetCare.Data;

namespace PMPetCare.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(customerId))
            {
                return Challenge();
            }

            var customer = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == customerId);

            var appointments = _context.Appointments
                .Where(a => a.CustomerId == customerId);

            ViewBag.CustomerName =
                string.IsNullOrWhiteSpace(customer?.FullName)
                    ? User.Identity?.Name
                    : customer.FullName;

            ViewBag.PetCount = await _context.Pets
                .CountAsync(p =>
                    p.CustomerId == customerId && !p.IsDeleted);

            ViewBag.AppointmentCount = await appointments.CountAsync();

            ViewBag.PendingCount = await appointments.CountAsync(a =>
                a.Status == "Pending");

            ViewBag.CompletedCount = await appointments.CountAsync(a =>
                a.Status == "Completed");

            var upcomingAppointments = await appointments
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Where(a =>
                    a.AppointmentDate >= DateTime.Now &&
                    a.Status != "Cancelled" &&
                    a.Status != "Completed")
                .OrderBy(a => a.AppointmentDate)
                .Take(5)
                .ToListAsync();

            var recentCareNotes = await _context.CareNotes
                .Include(c => c.Pet)
                .Include(c => c.Staff)
                .Where(c =>
                    c.Pet != null &&
                    c.Pet.CustomerId == customerId &&
                    !c.Pet.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentCareNotes = recentCareNotes;

            return View(upcomingAppointments);
        }
    }
}
