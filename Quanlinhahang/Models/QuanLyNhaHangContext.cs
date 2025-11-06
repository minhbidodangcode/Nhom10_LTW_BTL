using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

public partial class QuanLyNhaHangContext : DbContext
{
    public QuanLyNhaHangContext()
    {
    }

    public QuanLyNhaHangContext(DbContextOptions<QuanLyNhaHangContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BanPhong> BanPhongs { get; set; }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

    public virtual DbSet<DanhMucMon> DanhMucMons { get; set; }

    public virtual DbSet<DatBan> DatBans { get; set; }

    public virtual DbSet<HangThanhVien> HangThanhViens { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<KhungGio> KhungGios { get; set; }

    public virtual DbSet<LoaiBanPhong> LoaiBanPhongs { get; set; }

    public virtual DbSet<MonAn> MonAns { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-F4EAUJ7\\SQLEXPRESS;Database=Quanlinhahang;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BanPhong>(entity =>
        {
            entity.HasKey(e => e.BanPhongId).HasName("PK__BanPhong__B2D0E9578D911DBB");

            entity.Property(e => e.TrangThai).HasDefaultValue("Trống");

            entity.HasOne(d => d.LoaiBanPhong).WithMany(p => p.BanPhongs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BanPhong_LoaiBanPhong");
        });

        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.Property(e => e.SoLuong).HasDefaultValue(1);

            entity.HasOne(d => d.HoaDon).WithMany(p => p.ChiTietHoaDons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTHD_HoaDon");

            entity.HasOne(d => d.MonAn).WithMany(p => p.ChiTietHoaDons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTHD_MonAn");
        });

        modelBuilder.Entity<DanhMucMon>(entity =>
        {
            entity.HasKey(e => e.DanhMucId).HasName("PK__DanhMucM__1C53BA7B143DB335");
        });

        modelBuilder.Entity<DatBan>(entity =>
        {
            entity.HasKey(e => e.DatBanId).HasName("PK__DatBan__6A75F719D24099C3");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai).HasDefaultValue("Chờ xác nhận");

            entity.HasOne(d => d.BanPhong).WithMany(p => p.DatBans).HasConstraintName("FK_DatBan_BanPhong");

            entity.HasOne(d => d.KhachHang).WithMany(p => p.DatBans).HasConstraintName("FK_DatBan_KhachHang");

            entity.HasOne(d => d.KhungGio).WithMany(p => p.DatBans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatBan_KhungGio");
        });

        modelBuilder.Entity<HangThanhVien>(entity =>
        {
            entity.HasKey(e => e.HangThanhVienId).HasName("PK__HangThan__16F81D7AC4D8194B");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.HoaDonId).HasName("PK__HoaDon__6956CE69E82CDD11");

            entity.Property(e => e.NgayLap).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThaiThanhToan).HasDefaultValue("Chưa thanh toán");

            entity.HasOne(d => d.DatBan).WithMany(p => p.HoaDons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HoaDon_DatBan");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.HoaDons).HasConstraintName("FK_HoaDon_TaiKhoan");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.KhachHangId).HasName("PK__KhachHan__880F211B67C88DF0");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai).HasDefaultValue("Hoạt động");

            entity.HasOne(d => d.HangThanhVien).WithMany(p => p.KhachHangs).HasConstraintName("FK_KhachHang_HangThanhVien");
        });

        modelBuilder.Entity<KhungGio>(entity =>
        {
            entity.HasKey(e => e.KhungGioId).HasName("PK__KhungGio__CC9AB36AAE580C0B");
        });

        modelBuilder.Entity<LoaiBanPhong>(entity =>
        {
            entity.HasKey(e => e.LoaiBanPhongId).HasName("PK__LoaiBanP__BA742BBFCCC08C22");
        });

        modelBuilder.Entity<MonAn>(entity =>
        {
            entity.HasKey(e => e.MonAnId).HasName("PK__MonAn__272259EFA6D38CCB");

            entity.Property(e => e.TrangThai).HasDefaultValue("Còn bán");

            entity.HasOne(d => d.DanhMuc).WithMany(p => p.MonAns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MonAn_DanhMucMon");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.NhanVienId).HasName("PK__NhanVien__E27FD7EAC8D2C1B8");

            entity.Property(e => e.TrangThai).HasDefaultValue("Đang làm");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.NhanViens).HasConstraintName("FK_NhanVien_TaiKhoan");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.TaiKhoanId).HasName("PK__TaiKhoan__9A124B650CDB409D");

            entity.Property(e => e.TrangThai).HasDefaultValue("Hoạt động");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
