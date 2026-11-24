using Android17.Models;

namespace Android17.ViewModels;

public class ThongKeVM
{
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }

    public decimal DoanhThu { get; set; }      // đơn Hoàn thành
    public decimal DangXuLy { get; set; }      // đơn Chờ xác nhận / Đã xác nhận / Đang giao
    public int TongDon { get; set; }
    public int SoKhachDatHang { get; set; }
    public int SoSanPhamDangBan { get; set; }

    // Chỉ số 0..4 ứng với 5 trạng thái đơn
    public int[] SoDonTheoTrangThai { get; set; } = new int[5];

    public List<DoanhThuNgayItem> DoanhThuNgay { get; set; } = new();
    public List<BanChayItem> BanChay { get; set; } = new();

    public int NguongSapHet { get; set; }
    public List<SanPham> SapHet { get; set; } = new();
}

public class DoanhThuNgayItem
{
    public DateTime Ngay { get; set; }
    public int SoDon { get; set; }
    public decimal DoanhThu { get; set; }
}

public class BanChayItem
{
    public int Spid { get; set; }
    public string TenSp { get; set; } = "";
    public string Anh { get; set; } = "";
    public int SoLuong { get; set; }
    public decimal DoanhThu { get; set; }
}