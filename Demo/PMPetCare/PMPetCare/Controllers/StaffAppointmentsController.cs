
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMPetCare.Data;

namespace PMPetCare.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffAppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffAppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentStaffId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Staff chỉ xem lịch được phân công cho mình
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Where(a => a.StaffId == CurrentStaffId)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // Xem chi tiết lịch được giao
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == id &&
                    a.StaffId == CurrentStaffId);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // Cập nhật trạng thái lịch hẹn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == id &&
                    a.StaffId == CurrentStaffId);

            if (appointment == null)
                return NotFound();

            bool isValidTransition =
                (appointment.Status == "Assigned" &&
                    (status == "InProgress" || status == "Cancelled"))
                ||
                (appointment.Status == "InProgress" &&
                    (status == "Completed" || status == "Cancelled"));

            if (!isValidTransition)
            {
                TempData["ErrorMessage"] =
                    "Không thể chuyển sang trạng thái này.";

                return RedirectToAction(nameof(Details),
                    new { id });
            }

            appointment.Status = status;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Cập nhật trạng thái thành công!";

            return RedirectToAction(nameof(Details), new { id });
        }

    }

}
