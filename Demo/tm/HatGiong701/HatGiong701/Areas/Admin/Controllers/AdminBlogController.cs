using HatGiong701.Data;
using HatGiong701.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HatGiong701.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminBlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminBlogController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. Danh sách bài viết
        public async Task<IActionResult> Index()
        {
            var posts = await _context.BlogPosts
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
            return View(posts);
        }

        // 2. Trang Thêm bài viết mới
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogPost model, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    model.ImageUrl = await SaveImageAsync(imageFile);
                }
                model.CreatedDate = DateTime.Now;

                _context.BlogPosts.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã đăng bài viết cẩm nang mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 3. Trang Chỉnh sửa bài viết
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogPost model, IFormFile? imageFile)
        {
            if (id != model.BlogPostId) return NotFound();

            var existingPost = await _context.BlogPosts.FindAsync(id);
            if (existingPost == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingPost.Title = model.Title;
                existingPost.Category = model.Category;
                existingPost.Summary = model.Summary;
                existingPost.Content = model.Content;

                if (imageFile != null && imageFile.Length > 0)
                {
                    existingPost.ImageUrl = await SaveImageAsync(imageFile);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật bài viết thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 4. Xóa bài viết
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post != null)
            {
                _context.BlogPosts.Remove(post);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa bài viết.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            string folder = Path.Combine(_env.WebRootPath, "images", "blogs");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return "/images/blogs/" + fileName;
        }
    }
}
