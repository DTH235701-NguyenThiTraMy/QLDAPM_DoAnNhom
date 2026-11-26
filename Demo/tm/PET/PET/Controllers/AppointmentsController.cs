using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PET.Data;
using PET.Models;

namespace PET.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Include(a => a.User)
                .AsQueryable();

            // Customer chỉ xem lịch của mình; Admin xem tất cả
            if (!User.IsInRole("Admin"))
            {
                query = query.Where(a => a.UserId == userId);
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            // Đếm số lịch hẹn sắp diễn ra trong 24h tới để hiện banner nhắc lịch
            var tomorrow = DateTime.Now.AddHours(24);
            ViewBag.UpcomingCount = appointments.Count(a =>
                a.AppointmentDate >= DateTime.Now &&
                a.AppointmentDate <= tomorrow &&
                (a.Status == "Pending" || a.Status == "Confirmed"));

            return View(appointments);
        }

        // GET: Appointments/Create (Nhận sẵn serviceId nếu khách bấm từ trang Dịch vụ)
        public async Task<IActionResult> Create(int? serviceId)
        {
            var userId = _userManager.GetUserId(User);
            var userPets = await _context.Pets.Where(p => p.UserId == userId).ToListAsync();

            if (!userPets.Any() && !User.IsInRole("Admin"))
            {
                TempData["WarningMessage"] = "Bạn cần tạo hồ sơ cho bé cưng trước khi đặt lịch Spa nhé!";
                return RedirectToAction("Create", "Pets");
            }

            var petsList = User.IsInRole("Admin") ? await _context.Pets.ToListAsync() : userPets;
            ViewData["PetId"] = new SelectList(petsList, "Id", "Name");
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", serviceId);

            var model = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(1).Date.AddHours(9) // Mặc định 9h sáng ngày mai
            };

            return View(model);
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentDate,Notes,PetId,ServiceId")] Appointment appointment)
        {
            var userId = _userManager.GetUserId(User);
            appointment.UserId = userId!;
            appointment.Status = "Pending";
            appointment.IsReminded = false;

            // Bỏ qua lỗi validation của UserId vì được gán tự động ở server
            ModelState.Remove("UserId");

            if (appointment.AppointmentDate <= DateTime.Now)
            {
                ModelState.AddModelError("AppointmentDate", "Thời gian hẹn phải lớn hơn thời điểm hiện tại.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đặt lịch hẹn thành công! Fetch sẽ sớm xác nhận lịch của bé.";
                return RedirectToAction(nameof(Index));
            }

            var petsList = User.IsInRole("Admin")
                ? await _context.Pets.ToListAsync()
                : await _context.Pets.Where(p => p.UserId == userId).ToListAsync();

            ViewData["PetId"] = new SelectList(petsList, "Id", "Name", appointment.PetId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", appointment.ServiceId);
            return View(appointment);
        }

        // POST: Cập nhật trạng thái (Admin duyệt lịch / Khách hủy lịch)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && (appointment.UserId != userId || status != "Cancelled"))
            {
                return Forbid();
            }

            appointment.Status = status;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái lịch hẹn thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin gửi nhắc lịch cho khách (cập nhật IsReminded = true)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendReminder(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            appointment.IsReminded = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã gửi thông báo nhắc lịch Spa cho bé {appointment.Pet?.Name} tới {appointment.User?.Email}!";
            return RedirectToAction(nameof(Index));
        }
    }
}