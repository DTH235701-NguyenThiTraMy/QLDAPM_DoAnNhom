using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Android17.Models;

public partial class Android17Context : DbContext
{
    public Android17Context(DbContextOptions<Android17Context> options)
        : base(options)
    {
    }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<DonHangChiTiet> DonHangChiTiets { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<LoaiHang> LoaiHangs { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NhanHang> NhanHangs { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.Dhid).HasName("PK__DonHang__2971B07D3812BE27");

            entity.Property(e => e.NgayTaoDon).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Nd).WithMany(p => p.DonHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DonHang_NguoiDung");
        });

        modelBuilder.Entity<DonHangChiTiet>(entity =>
        {
            entity.HasKey(e => e.Ctid).HasName("PK__DonHangC__F4AA1BE014219A5A");

            entity.HasOne(d => d.Dh).WithMany(p => p.DonHangChiTiets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DHChiTiet_DonHang");

            entity.HasOne(d => d.Sp).WithMany(p => p.DonHangChiTiets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DHChiTiet_SanPham");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.GioId).HasName("PK__GioHang__06555308293E4752");

            entity.HasOne(d => d.Nd).WithMany(p => p.GioHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GioHang_NguoiDung");

            entity.HasOne(d => d.Sp).WithMany(p => p.GioHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GioHang_SanPham");
        });

        modelBuilder.Entity<LoaiHang>(entity =>
        {
            entity.HasKey(e => e.LoaiId).HasName("PK__LoaiHang__482510E241FE89EE");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.Ndid).HasName("PK__NguoiDun__D0BE0150F94F265B");

            entity.Property(e => e.TgTaoTk).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<NhanHang>(entity =>
        {
            entity.HasKey(e => e.NhanHangId).HasName("PK__NhanHang__5B9B9CEA88525449");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.Spid).HasName("PK__SanPham__F43060C90DAE0743");

            entity.Property(e => e.DangBan).HasDefaultValue(true);

            entity.HasOne(d => d.Loai).WithMany(p => p.SanPhams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SanPham_LoaiHang");

            entity.HasOne(d => d.NhanHang).WithMany(p => p.SanPhams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SanPham_NhanHang");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
