using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Android17.Models;

[Table("DonHang")]
[Index("Ndid", Name = "IX_DonHang_ND")]
public partial class DonHang
{
    [Key]
    [Column("DHID")]
    public int Dhid { get; set; }

    [Column("NDID")]
    public int Ndid { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime NgayTaoDon { get; set; }

    [StringLength(100)]
    public string NguoiNhan { get; set; } = null!;

    [Column("SDTNguoiNhan")]
    [StringLength(20)]
    public string SdtnguoiNhan { get; set; } = null!;

    [Column("DChi")]
    [StringLength(255)]
    public string Dchi { get; set; } = null!;

    [StringLength(255)]
    public string? GhiChu { get; set; }

    [Column("PThucThanhToan")]
    [StringLength(50)]
    public string PthucThanhToan { get; set; } = null!;

    [Column("TongTienSP", TypeName = "decimal(18, 0)")]
    public decimal TongTienSp { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal PhiVanChuyen { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal ThanhTien { get; set; }

    public int TrangThai { get; set; }

    [Column("TG_XacNhan", TypeName = "datetime")]
    public DateTime? TgXacNhan { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayGiaoDuKien { get; set; }

    [Column("TG_DaGiao", TypeName = "datetime")]
    public DateTime? TgDaGiao { get; set; }

    [InverseProperty("Dh")]
    public virtual ICollection<DonHangChiTiet> DonHangChiTiets { get; set; } = new List<DonHangChiTiet>();

    [ForeignKey("Ndid")]
    [InverseProperty("DonHangs")]
    public virtual NguoiDung Nd { get; set; } = null!;
}
