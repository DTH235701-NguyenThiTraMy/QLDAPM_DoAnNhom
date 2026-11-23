using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMPetCare.Data;
using PMPetCare.Models;

namespace PMPetCare.Controllers;

[Authorize(Roles = "Admin")]
public class AdminServicesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminServicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: AdminServices
    public async Task<IActionResult> Index()
    {
        var services = await _context.Services.ToListAsync();
        return View(services);
    }

    // GET: AdminServices/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.ServiceId == id);

        if (service == null)
            return NotFound();

        return View(service);
    }

    // GET: AdminServices/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: AdminServices/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("ServiceName,Description,Price,Duration,IsActive")]
        Service service)
    {
        if (ModelState.IsValid)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(service);
    }

    // GET: AdminServices/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var service = await _context.Services.FindAsync(id);

        if (service == null)
            return NotFound();

        return View(service);
    }

    // POST: AdminServices/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("ServiceId,ServiceName,Description,Price,Duration,IsActive")]
        Service service)
    {
        if (id != service.ServiceId)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(service);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(service.ServiceId))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(service);
    }

    // POST: AdminServices/ToggleActive
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int serviceId)
    {
        var service = await _context.Services.FindAsync(serviceId);

        if (service == null)
            return NotFound();

        service.IsActive = !service.IsActive;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private bool ServiceExists(int id)
    {
        return _context.Services.Any(s => s.ServiceId == id);
    }
}
