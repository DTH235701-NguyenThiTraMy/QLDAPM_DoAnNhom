
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PMPetCare.Data;
using PMPetCare.Models;

namespace PMPetCare.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminAppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminAppointmentsController(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AdminAppointments
        // Admin xem tất cả lịch hẹn
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(appointments);
        }

        // GET: AdminAppointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .Include(a => a.Staff)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // GET: AdminAppointments/AssignStaff/5
        public async Task<IActionResult> AssignStaff(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound();

            if (appointment.Status != "Pending")
            {
                TempData["ErrorMessage"] =
                    "Chỉ có thể phân công nhân viên cho lịch đang chờ xử lý.";

                return RedirectToAction(nameof(Index));
            }

            await LoadStaffList();

            ViewBag.Appointment = appointment;

            return View();
        }

        // POST: AdminAppointments/AssignStaff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStaff(
            int id, string staffId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound();

            if (appointment.Status != "Pending")
            {
                TempData["ErrorMessage"] =
                    "Lịch hẹn không còn ở trạng thái chờ xử lý.";

                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(staffId))
            {
                ModelState.AddModelError(
                    "staffId", "Vui lòng chọn nhân viên.");

                await LoadStaffList();
                ViewBag.Appointment = appointment;

                return View();
            }

            // Kiểm tra tài khoản có tồn tại và thực sự thuộc vai trò Staff
            var staff = await _userManager.FindByIdAsync(staffId);

            if (staff == null ||
                !await _userManager.IsInRoleAsync(staff, "Staff"))
            {
                ModelState.AddModelError(
                    "staffId", "Nhân viên được chọn không hợp lệ.");

                await LoadStaffList();
                ViewBag.Appointment = appointment;

                return View();
            }
            // Lấy thời lượng dịch vụ của lịch đang phân công
            var currentService = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == appointment.ServiceId);

            if (currentService == null)
                return NotFound();

            // Tính thời gian kết thúc của lịch hiện tại
            var currentStart = appointment.AppointmentDate;
            var currentEnd = currentStart.AddMinutes(currentService.Duration);

            // Kiểm tra lịch khác của nhân viên có bị chồng lấn thời gian không
            var isOverlapping = await _context.Appointments
                .Include(a => a.Service)
                .AnyAsync(a =>
                    a.AppointmentId != appointment.AppointmentId &&
                    a.StaffId == staff.Id &&
                    a.Status != "Cancelled" &&
                    a.Service != null &&
                    a.AppointmentDate < currentEnd &&
                    a.AppointmentDate.AddMinutes(a.Service.Duration) > currentStart
                );

            if (isOverlapping)
            {
                ModelState.AddModelError(
                    "staffId",
                    "Nhân viên này đã có lịch hẹn trùng thời gian. Vui lòng chọn nhân viên khác.");

                await LoadStaffList();
                ViewBag.Appointment = appointment;

                return View();
            }

            appointment.StaffId = staff.Id;
            appointment.Status = "Assigned";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Phân công nhân viên thành công!";

            return RedirectToAction(nameof(Index));
        }

        // Lấy danh sách tài khoản có vai trò Staff
        private async Task LoadStaffList()
        {
            var staffList = await _userManager.GetUsersInRoleAsync("Staff");

            ViewBag.StaffId = new SelectList(
                staffList.OrderBy(s => s.FullName),
                "Id",
                "FullName");
        }
    }
}
