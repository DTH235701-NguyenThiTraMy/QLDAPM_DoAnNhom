using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Android17.Models;

[Table("NguoiDung")]
[Index("IdentityUserId", Name = "UQ_NguoiDung_IdentityUserId", IsUnique = true)]
public partial class NguoiDung
{
    [Key]
    [Column("NDID")]
    public int Ndid { get; set; }

    public string IdentityUserId { get; set; } = null!;

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    [Column("SDT")]
    [StringLength(20)]
    public string? Sdt { get; set; }

    [Column("DChi")]
    [StringLength(255)]
    public string? Dchi { get; set; }

    [Column("TG_TaoTK", TypeName = "datetime")]
    public DateTime TgTaoTk { get; set; }

    [InverseProperty("Nd")]
    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    [InverseProperty("Nd")]
    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
}
