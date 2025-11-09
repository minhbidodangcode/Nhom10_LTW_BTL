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
            entity.HasKey(e => e.BanPhongId).HasName("PK__BanPhong__B2D0E957D822BFEF");

            entity.ToTable("BanPhong");

            entity.Property(e => e.BanPhongId).HasColumnName("BanPhongID");
            entity.Property(e => e.LoaiBanPhongId).HasColumnName("LoaiBanPhongID");
            entity.Property(e => e.TenBanPhong).HasMaxLength(50);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Trống");

            entity.HasOne(d => d.LoaiBanPhong).WithMany(p => p.BanPhongs)
                .HasForeignKey(d => d.LoaiBanPhongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BanPhong_LoaiBanPhong");
        });

        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => new { e.HoaDonId, e.MonAnId });

            entity.ToTable("ChiTietHoaDon");

            entity.Property(e => e.HoaDonId).HasColumnName("HoaDonID");
            entity.Property(e => e.MonAnId).HasColumnName("MonAnID");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SoLuong).HasDefaultValue(1);
            entity.Property(e => e.ThanhTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.HoaDon).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.HoaDonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTHD_HoaDon");

            entity.HasOne(d => d.MonAn).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MonAnId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTHD_MonAn");
        });

        modelBuilder.Entity<DanhMucMon>(entity =>
        {
            entity.HasKey(e => e.DanhMucId).HasName("PK__DanhMucM__1C53BA7B82A0E29B");

            entity.ToTable("DanhMucMon");

            entity.Property(e => e.DanhMucId).HasColumnName("DanhMucID");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenDanhMuc).HasMaxLength(100);
        });

        modelBuilder.Entity<DatBan>(entity =>
        {
            entity.HasKey(e => e.DatBanId).HasName("PK__DatBan__6A75F719655FFEEE");

            entity.ToTable("DatBan");

            entity.Property(e => e.DatBanId).HasColumnName("DatBanID");
            entity.Property(e => e.BanPhongId).HasColumnName("BanPhongID");
            entity.Property(e => e.KhachHangId).HasColumnName("KhachHangID");
            entity.Property(e => e.KhungGioId).HasColumnName("KhungGioID");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TongTienDuKien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Chờ xác nhận");

            entity.HasOne(d => d.BanPhong).WithMany(p => p.DatBans)
                .HasForeignKey(d => d.BanPhongId)
                .HasConstraintName("FK_DatBan_BanPhong");

            entity.HasOne(d => d.KhachHang).WithMany(p => p.DatBans)
                .HasForeignKey(d => d.KhachHangId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_DatBan_KhachHang");

            entity.HasOne(d => d.KhungGio).WithMany(p => p.DatBans)
                .HasForeignKey(d => d.KhungGioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatBan_KhungGio");
        });

        modelBuilder.Entity<HangThanhVien>(entity =>
        {
            entity.HasKey(e => e.HangThanhVienId).HasName("PK__HangThan__16F81D7A09D4F66D");

            entity.ToTable("HangThanhVien");

            entity.Property(e => e.HangThanhVienId).HasColumnName("HangThanhVienID");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenHang).HasMaxLength(50);
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.HoaDonId).HasName("PK__HoaDon__6956CE69B8A08F25");

            entity.ToTable("HoaDon");

            entity.HasIndex(e => e.NgayLap, "IX_HoaDon_NgayLap");

            entity.HasIndex(e => e.TrangThai, "IX_HoaDon_TrangThai");

            entity.Property(e => e.HoaDonId).HasColumnName("HoaDonID");
            entity.Property(e => e.DatBanId).HasColumnName("DatBanID");
            entity.Property(e => e.GiamGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HinhThucThanhToan).HasMaxLength(50);
            entity.Property(e => e.LoaiDichVu).HasMaxLength(50);
            entity.Property(e => e.NgayLap).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Chưa xác nhận");
            entity.Property(e => e.Vat)
                .HasDefaultValue(0.10m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("VAT");

            entity.HasOne(d => d.DatBan).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.DatBanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HoaDon_DatBan");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.TaiKhoanId)
                .HasConstraintName("FK_HoaDon_TaiKhoan");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.KhachHangId).HasName("PK__KhachHan__880F211BC7C09F49");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.Email, "IX_KhachHang_Email");

            entity.Property(e => e.KhachHangId).HasColumnName("KhachHangID");
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HangThanhVienId).HasColumnName("HangThanhVienID");
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Hoạt động");

            entity.HasOne(d => d.HangThanhVien).WithMany(p => p.KhachHangs)
                .HasForeignKey(d => d.HangThanhVienId)
                .HasConstraintName("FK_KhachHang_HangThanhVien");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.KhachHangs)
                .HasForeignKey(d => d.TaiKhoanId)
                .HasConstraintName("FK_KhachHang_TaiKhoan");
        });

        modelBuilder.Entity<KhungGio>(entity =>
        {
            entity.HasKey(e => e.KhungGioId).HasName("PK__KhungGio__CC9AB36A058A95CE");

            entity.ToTable("KhungGio");

            entity.Property(e => e.KhungGioId).HasColumnName("KhungGioID");
            entity.Property(e => e.TenKhungGio).HasMaxLength(50);
        });

        modelBuilder.Entity<LoaiBanPhong>(entity =>
        {
            entity.HasKey(e => e.LoaiBanPhongId).HasName("PK__LoaiBanP__BA742BBF896B5143");

            entity.ToTable("LoaiBanPhong");

            entity.Property(e => e.LoaiBanPhongId).HasColumnName("LoaiBanPhongID");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.PhuThu).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<MonAn>(entity =>
        {
            entity.HasKey(e => e.MonAnId).HasName("PK__MonAn__272259EF86839E6D");

            entity.ToTable("MonAn");

            entity.Property(e => e.MonAnId).HasColumnName("MonAnID");
            entity.Property(e => e.DanhMucId).HasColumnName("DanhMucID");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HinhAnhUrl)
                .HasMaxLength(255)
                .HasColumnName("HinhAnhURL");
            entity.Property(e => e.LoaiMon).HasMaxLength(50);
            entity.Property(e => e.TenMon).HasMaxLength(150);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Còn bán");

            entity.HasOne(d => d.DanhMuc).WithMany(p => p.MonAns)
                .HasForeignKey(d => d.DanhMucId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MonAn_DanhMucMon");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.NhanVienId).HasName("PK__NhanVien__E27FD7EABE6B448F");

            entity.ToTable("NhanVien");

            entity.Property(e => e.NhanVienId).HasColumnName("NhanVienID");
            entity.Property(e => e.ChucVu).HasMaxLength(50);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Đang làm");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.TaiKhoanId)
                .HasConstraintName("FK_NhanVien_TaiKhoan");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.TaiKhoanId).HasName("PK__TaiKhoan__9A124B65019721A7");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.Email, "IX_TaiKhoan_Email");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC0AA38DA51").IsUnique();

            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.MatKhauHash).HasMaxLength(255);
            entity.Property(e => e.TenDangNhap).HasMaxLength(50);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Hoạt động");
            entity.Property(e => e.VaiTro).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
