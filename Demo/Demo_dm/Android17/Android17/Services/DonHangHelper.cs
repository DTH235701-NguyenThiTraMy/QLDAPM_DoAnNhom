using Android17.Models;
using Microsoft.EntityFrameworkCore;

namespace Android17.Services;

public static class DonHangHelper
{
    // Hủy đơn và hoàn lại tồn kho.
    // Trả về false nếu đơn không còn ở trạng thái được phép hủy.
    public static async Task<bool> HuyDonAsync(Android17Context context, int dhid, bool choPhepDaXacNhan)
    {
        await using var tran = await context.Database.BeginTransactionAsync();

        // Chỉ đổi sang "Đã hủy" khi đơn còn đúng trạng thái cho phép.
        // Nếu cùng lúc có người khác xử lý đơn thì chỉ một bên thành công.
        int dong = await context.DonHangs
            .Where(d => d.Dhid == dhid && (d.TrangThai == 0 || (choPhepDaXacNhan && d.TrangThai == 1)))
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.TrangThai, 4));
        if (dong == 0) return false;

        var chiTiet = await context.DonHangChiTiets
            .Where(c => c.Dhid == dhid)
            .Select(c => new { c.Spid, c.Sluong })
            .ToListAsync();

        foreach (var c in chiTiet)
        {
            int spid = c.Spid;
            int sl = c.Sluong;
            await context.SanPhams
                .Where(s => s.Spid == spid)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.TonKho, x => x.TonKho + sl));
        }

        await tran.CommitAsync();
        return true;
    }
}