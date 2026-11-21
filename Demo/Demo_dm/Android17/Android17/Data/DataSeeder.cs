using Android17.Models;
using Microsoft.AspNetCore.Identity;

namespace Android17.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var db = services.GetRequiredService<Android17Context>();

        foreach (var role in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await TaoTaiKhoan(userManager, db, "admin@phoneshop.vn", "Admin@123", "Quản trị viên", "Admin");
        await TaoTaiKhoan(userManager, db, "khach1@gmail.com", "User@123", "Nguyễn Văn An", "Customer");
        await TaoTaiKhoan(userManager, db, "khach2@gmail.com", "User@123", "Trần Thị Bình", "Customer");
    }

    private static async Task TaoTaiKhoan(UserManager<IdentityUser> um, Android17Context db,
        string email, string matKhau, string hoTen, string vaiTro)
    {
        var user = await um.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var kq = await um.CreateAsync(user, matKhau);
            if (!kq.Succeeded) return;
            await um.AddToRoleAsync(user, vaiTro);
        }

        // Tạo hồ sơ khách hàng (bảng NguoiDung) nếu chưa có
        var tapHoSo = db.Set<NguoiDung>();
        if (!tapHoSo.Any(n => n.IdentityUserId == user.Id))
        {
            tapHoSo.Add(new NguoiDung { IdentityUserId = user.Id, HoTen = hoTen });
            await db.SaveChangesAsync();
        }
    }
}