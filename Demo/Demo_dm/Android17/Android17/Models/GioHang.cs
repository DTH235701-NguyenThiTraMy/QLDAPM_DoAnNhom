using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Android17.Models;

[Table("GioHang")]
[Index("Ndid", "Spid", Name = "UQ_GioHang_ND_SP", IsUnique = true)]
public partial class GioHang
{
    [Key]
    [Column("GioID")]
    public int GioId { get; set; }

    [Column("NDID")]
    public int Ndid { get; set; }

    [Column("SPID")]
    public int Spid { get; set; }

    [Column("SLuong")]
    public int Sluong { get; set; }

    [ForeignKey("Ndid")]
    [InverseProperty("GioHangs")]
    public virtual NguoiDung Nd { get; set; } = null!;

    [ForeignKey("Spid")]
    [InverseProperty("GioHangs")]
    public virtual SanPham Sp { get; set; } = null!;
}
