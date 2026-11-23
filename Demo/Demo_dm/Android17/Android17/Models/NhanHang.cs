using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Android17.Models;

[Table("NhanHang")]
public partial class NhanHang
{
    [Key]
    [Column("NhanHangID")]
    public int NhanHangId { get; set; }

    [StringLength(100)]
    public string TenNhanHang { get; set; } = null!;

    [InverseProperty("NhanHang")]
    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
