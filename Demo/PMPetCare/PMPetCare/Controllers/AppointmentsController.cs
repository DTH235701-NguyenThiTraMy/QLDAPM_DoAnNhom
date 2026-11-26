
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PMPetCare.Data;
using PMPetCare.Models;

namespace PMPetCare.Controllers
{
    [Authorize(Roles = "Customer")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy ID của Customer đang đăng nhập
        private string CurrentCustomerId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: Appointments
        // Chỉ xem lịch hẹn của chính mình
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Where(a => a.CustomerId == CurrentCustomerId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(appointments);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == id &&
                    a.CustomerId == CurrentCustomerId);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create()
        {
            await LoadCreateData();
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int PetId,
            int ServiceId,
            DateTime AppointmentDate,
            string? Note)
        {
            // Kiểm tra thời gian đặt lịch
            if (AppointmentDate <= DateTime.Now)
            {
                ModelState.AddModelError(
                    nameof(AppointmentDate),
                    "Vui lòng chọn thời gian trong tương lai.");
            }

            // Kiểm tra thú cưng có thuộc Customer đang đăng nhập
            var pet = await _context.Pets
                .FirstOrDefaultAsync(p =>
                    p.PetId == PetId &&
                    p.CustomerId == CurrentCustomerId &&
                    !p.IsDeleted);

            if (pet == null)
            {
                ModelState.AddModelError(
                    nameof(PetId),
                    "Thú cưng không hợp lệ hoặc không thuộc tài khoản của bạn.");
            }

            // Chỉ cho phép chọn dịch vụ đang hoạt động
            var service = await _context.Services
                .FirstOrDefaultAsync(s =>
                    s.ServiceId == ServiceId &&
                    s.IsActive);

            if (service == null)
            {
                ModelState.AddModelError(
                    nameof(ServiceId),
                    "Dịch vụ không tồn tại hoặc hiện không hoạt động.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCreateData();
                return View();
            }

            // Tạo lịch hẹn bằng dữ liệu đã xác thực
            var appointment = new Appointment
            {
                CustomerId = CurrentCustomerId,
                PetId = pet!.PetId,
                ServiceId = service!.ServiceId,
                AppointmentDate = AppointmentDate,
                Note = Note,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                StaffId = null
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đặt lịch thành công!";

            return RedirectToAction(nameof(Index));
        }

        // Nạp danh sách thú cưng và dịch vụ cho form đặt lịch
        private async Task LoadCreateData()
        {
            var pets = await _context.Pets
                .Where(p =>
                    p.CustomerId == CurrentCustomerId &&
                    !p.IsDeleted)
                .OrderBy(p => p.PetName)
                .ToListAsync();

            var services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.ServiceName)
                .ToListAsync();

            ViewBag.PetId = new SelectList(
                pets, "PetId", "PetName");

            ViewBag.ServiceId = new SelectList(
                services, "ServiceId", "ServiceName");
        }
    }
}
