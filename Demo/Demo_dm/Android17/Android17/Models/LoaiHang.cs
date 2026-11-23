using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Android17.Models;

[Table("LoaiHang")]
public partial class LoaiHang
{
    [Key]
    [Column("LoaiID")]
    public int LoaiId { get; set; }

    [StringLength(100)]
    public string TenLoai { get; set; } = null!;

    [StringLength(255)]
    public string? MoTa { get; set; }

    [InverseProperty("Loai")]
    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
