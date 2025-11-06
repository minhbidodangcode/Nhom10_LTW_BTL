using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

[Table("NhanVien")]
public partial class NhanVien
{
    [Key]
    [Column("NhanVienID")]
    public int NhanVienId { get; set; }

    [Column("TaiKhoanID")]
    public int? TaiKhoanId { get; set; }

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    [StringLength(20)]
    public string? SoDienThoai { get; set; }

    [StringLength(50)]
    public string? ChucVu { get; set; }

    public DateOnly? NgayVaoLam { get; set; }

    [StringLength(20)]
    public string TrangThai { get; set; } = null!;

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("NhanViens")]
    public virtual TaiKhoan? TaiKhoan { get; set; }
}
