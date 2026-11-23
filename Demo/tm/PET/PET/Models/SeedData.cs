using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PET.Data;

namespace PET.Models
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Tạo các quyền (Roles): Admin và Customer
            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Tạo tài khoản Admin mặc định
            var adminEmail = "admin@petcare.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Quản trị viên Hệ thống",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Thêm dữ liệu Dịch vụ mẫu nếu bảng Services đang trống
            if (!context.Services.Any())
            {
                context.Services.AddRange(
                    new Service { Name = "Tắm gội & Vệ sinh cơ bản", Description = "Tắm sữa tắm chuyên dụng, sấy khô, vệ sinh tai và cắt móng.", Price = 150000, DurationMinutes = 45 },
                    new Service { Name = "Cắt tỉa lông tạo kiểu", Description = "Cắt tỉa gọn gàng theo giống thú cưng.", Price = 300000, DurationMinutes = 90 },
                    new Service { Name = "Khám sức khỏe tổng quát", Description = "Kiểm tra tim mạch, da liễu, tai mũi họng định kỳ.", Price = 200000, DurationMinutes = 30 }
                );
            }

            // 4. Thêm dữ liệu Sản phẩm mẫu nếu bảng Products đang trống
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product { Name = "Hạt Royal Canin Poodle Adult 1.5kg", Category = "Thức ăn", Description = "Thức ăn hạt chuyên biệt cho chó Poodle trưởng thành.", Price = 380000, StockQuantity = 50 },
                    new Product { Name = "Cát vệ sinh đậu nành cho mèo 6L", Category = "Phụ kiện", Description = "Khử mùi tốt, vón cục nhanh, an toàn cho mèo.", Price = 120000, StockQuantity = 100 }
                );
            }

            await context.SaveChangesAsync();
        }
    }
}