using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMPetCare.Models;
using PMPetCare.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace PMPetCare.Controllers;

[Authorize (Roles ="Customer")]
public class PetsController : Controller
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public PetsController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: PETS
    public async Task<IActionResult> Index()    
    {
        var pets = await _context.Pets
            .Where(p => 
            p.CustomerId == CurrentUserId && 
            !p.IsDeleted)
            .ToListAsync();
        return View(pets);
    }

    // GET: PETS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
       var pet = await _context.Pets
            .FirstOrDefaultAsync(p => 
            p.PetId == id && 
            p.CustomerId == CurrentUserId && 
            !p.IsDeleted);

        if (pet == null)
            return NotFound();

        return View(pet);
       
    }

    // GET: PETS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: PETS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("PetName,Species,Breed,Age,Gender,Description")] Pet pet, IFormFile? image)
    {
        ModelState.Remove(nameof(Pet.CustomerId));

        if (!ModelState.IsValid)
             return View(pet);
        try 
        {
           pet.ImagePath = await SavePetImage(image);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("image", ex.Message);
            return View(pet);
        }
        pet.CustomerId = CurrentUserId;

        _context.Add(pet);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }

    // GET: PETS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => 
            p.PetId == id && 
            p.CustomerId == CurrentUserId && !p.IsDeleted);
        if (pet == null)
        {
            return NotFound();
        }
        return View(pet);
    }

    // POST: PETS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("PetName,Species,Breed,Age,Gender,Description")] Pet pet, IFormFile? image)
    {

        var existingPet = await _context.Pets
         .FirstOrDefaultAsync(p =>
         p.PetId == id &&
         p.CustomerId == CurrentUserId &&
         !p.IsDeleted);
        if (existingPet == null)
        {
            return NotFound();
        }
        ModelState.Remove(nameof(Pet.CustomerId));
        if (!ModelState.IsValid)
        {
            pet.PetId = existingPet.PetId;
            pet.ImagePath = existingPet.ImagePath;
            return View(pet);
        }
              
     try
        {
            var newImagePath = await SavePetImage(image);
            if (newImagePath != null)
            {
                existingPet.ImagePath = newImagePath;
            }
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("image", ex.Message);
            pet.ImagePath = existingPet.ImagePath;
            return View(pet);
        } 
        existingPet.PetName = pet.PetName;
        existingPet.Species = pet.Species;
        existingPet.Breed = pet.Breed;
        existingPet.Age = pet.Age;
        existingPet.Gender = pet.Gender;
        existingPet.Description = pet.Description;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));

    }

    // GET: PETS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) 
            return NotFound();

        var pet = await _context.Pets
        .FirstOrDefaultAsync(p => 
            p.PetId == id && 
            p.CustomerId == CurrentUserId && 
            !p.IsDeleted);
        if (pet == null) 
            return NotFound();

        return View(pet);
    }

    // POST: PETS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null) 
            return NotFound();

        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => 
            p.PetId == id && 
            p.CustomerId == CurrentUserId && 
            !p.IsDeleted);

        if (pet == null)
            return NotFound();

        pet.IsDeleted = true;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));

    }

    private async Task<string?> SavePetImage(IFormFile? image)
    {
        if (image == null || image.Length == 0)
            return null;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");

        if (image.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("Ảnh không được vượt quá 5 MB.");

        var folder = Path.Combine(_environment.WebRootPath, "images", "pets");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return $"/images/pets/{fileName}";
    }

}
