
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMPetCare.Data;
using PMPetCare.Models;

namespace PMPetCare.Controllers
{
    [Authorize(Roles = "Staff")]
    public class CareNotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CareNotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentStaffId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: CareNotes/Create/5 (5 là mã lịch hẹn)
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == id &&
                    a.StaffId == CurrentStaffId &&
                    (a.Status == "InProgress" ||
                     a.Status == "Completed"));

            if (appointment == null)
                return NotFound();

            ViewBag.Appointment = appointment;

            return View();
        }

        // POST: CareNotes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int id, string content, DateTime? reminderDate)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == id &&
                    a.StaffId == CurrentStaffId &&
                    (a.Status == "InProgress" ||
                     a.Status == "Completed"));

            if (appointment == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError(
                    "content", "Vui lòng nhập nội dung chăm sóc.");
            }

            if (reminderDate.HasValue &&
                reminderDate.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "reminderDate",
                    "Ngày nhắc lại không được ở trong quá khứ.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Appointment = appointment;
                return View();
            }

            var careNote = new CareNote
            {
                AppointmentId = appointment.AppointmentId,
                PetId = appointment.PetId,
                StaffId = CurrentStaffId,
                Content = content.Trim(),
                CreatedAt = DateTime.Now,
                ReminderDate = reminderDate
            };

            _context.CareNotes.Add(careNote);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Ghi nhận thông tin chăm sóc thành công!";

            return RedirectToAction(
                "Details",
                "StaffAppointments",
                new { id = appointment.AppointmentId });
        }
    }
}
