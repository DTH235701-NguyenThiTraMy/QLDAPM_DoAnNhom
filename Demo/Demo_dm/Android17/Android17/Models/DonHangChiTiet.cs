using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Android17.Models;

[Table("DonHangChiTiet")]
[Index("Dhid", Name = "IX_DHChiTiet_DH")]
public partial class DonHangChiTiet
{
    [Key]
    [Column("CTID")]
    public int Ctid { get; set; }

    [Column("DHID")]
    public int Dhid { get; set; }

    [Column("SPID")]
    public int Spid { get; set; }

    [Column("SLuong")]
    public int Sluong { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal GiaLucDat { get; set; }

    [ForeignKey("Dhid")]
    [InverseProperty("DonHangChiTiets")]
    public virtual DonHang Dh { get; set; } = null!;

    [ForeignKey("Spid")]
    [InverseProperty("DonHangChiTiets")]
    public virtual SanPham Sp { get; set; } = null!;
}
