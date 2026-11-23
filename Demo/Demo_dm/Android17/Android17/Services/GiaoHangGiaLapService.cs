using Android17.Models;
using Microsoft.EntityFrameworkCore;

namespace Android17.Services;

// Chạy ngầm cùng website: cứ 5 giây cập nhật trạng thái các đơn đang giao theo thời gian
public class GiaoHangGiaLapService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GiaoHangGiaLapService> _logger;

    public GiaoHangGiaLapService(IServiceScopeFactory scopeFactory, ILogger<GiaoHangGiaLapService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<Android17Context>();
                await CapNhatAsync(context);
            }
            catch (Exception ex)
            {
                // Có lỗi thì ghi log rồi chạy tiếp, không làm sập website
                _logger.LogWarning(ex, "Không cập nhật được trạng thái giao hàng giả lập.");
            }

            try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    private static async Task CapNhatAsync(Android17Context context)
    {
        var now = DateTime.Now;
        var ds = await context.DonHangs
            .AsNoTracking()
            .Where(d => (d.TrangThai == 1 || d.TrangThai == 2) && d.TgXacNhan != null && d.NgayGiaoDuKien != null)
            .Select(d => new { d.Dhid, d.TrangThai, d.TgXacNhan, d.NgayGiaoDuKien })
            .ToListAsync();

        foreach (var d in ds)
        {
            var batDau = d.TgXacNhan!.Value;
            var ketThuc = d.NgayGiaoDuKien!.Value;
            var moc = batDau + (ketThuc - batDau) / 4;   // sau 1/4 thời gian thì chuyển sang "Đang giao"

            int moi = now >= ketThuc ? 3 : (now >= moc ? 2 : 1);
            if (moi == d.TrangThai) continue;

            int id = d.Dhid;
            int cu = d.TrangThai;
            // Điều kiện TrangThai == cu: nếu đơn vừa bị hủy thì không ghi đè
            var dong = context.DonHangs.Where(x => x.Dhid == id && x.TrangThai == cu);

            if (moi == 3)
                await dong.ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.TrangThai, 3)
                    .SetProperty(x => x.TgDaGiao, (DateTime?)ketThuc));
            else
                await dong.ExecuteUpdateAsync(s => s.SetProperty(x => x.TrangThai, moi));
        }
    }
}