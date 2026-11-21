using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Android17.Models;

[Table("SanPham")]
[Index("LoaiId", Name = "IX_SanPham_Loai")]
[Index("NhanHangId", Name = "IX_SanPham_NhanHang")]
public partial class SanPham
{
    [Key]
    [Column("SPID")]
    public int Spid { get; set; }

    [Column("TenSP")]
    [StringLength(200)]
    public string TenSp { get; set; } = null!;

    [Column("LoaiID")]
    public int LoaiId { get; set; }

    [Column("NhanHangID")]
    public int NhanHangId { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal Gia { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal? GiaGoc { get; set; }

    public int TonKho { get; set; }

    [StringLength(255)]
    public string Anh { get; set; } = null!;

    [StringLength(500)]
    public string? MoTaNgan { get; set; }

    public string? ThongSo { get; set; }

    public bool NoiBat { get; set; }

    public bool DangBan { get; set; }

    [InverseProperty("Sp")]
    public virtual ICollection<DonHangChiTiet> DonHangChiTiets { get; set; } = new List<DonHangChiTiet>();

    [InverseProperty("Sp")]
    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    [ForeignKey("LoaiId")]
    [InverseProperty("SanPhams")]
    public virtual LoaiHang Loai { get; set; } = null!;

    [ForeignKey("NhanHangId")]
    [InverseProperty("SanPhams")]
    public virtual NhanHang NhanHang { get; set; } = null!;
}
