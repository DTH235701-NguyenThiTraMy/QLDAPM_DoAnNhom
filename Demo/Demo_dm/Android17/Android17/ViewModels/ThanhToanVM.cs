using System.ComponentModel.DataAnnotations;

namespace Android17.ViewModels;

// Chỉ chứa các ô khách nhập trên form đặt hàng
public class ThanhToanVM
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận.")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    public string NguoiNhan { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0.")]
    public string Sdt { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng.")]
    [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
    public string DiaChi { get; set; } = "";

    [StringLength(255, ErrorMessage = "Ghi chú tối đa 255 ký tự.")]
    public string? GhiChu { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
    public string PhuongThuc { get; set; } = "COD";
}