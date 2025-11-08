using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

[Table("TaiKhoan")]
[Index("TenDangNhap", Name = "UQ__TaiKhoan__55F68FC000348C48", IsUnique = true)]
public partial class TaiKhoan
{
    [Key]
    [Column("TaiKhoanID")]
    public int TaiKhoanId { get; set; }

    [StringLength(50)]
    public string TenDangNhap { get; set; } = null!;

    [StringLength(255)]
    public string MatKhauHash { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string VaiTro { get; set; } = null!;

    [StringLength(20)]
    public string TrangThai { get; set; } = null!;

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();

    [InverseProperty("TaiKhoan")]
    public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();

}
