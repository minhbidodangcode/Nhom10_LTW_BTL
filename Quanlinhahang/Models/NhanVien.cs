using System;
using System.Collections.Generic;

namespace Quanlinhahang.Models;

public partial class NhanVien
{
    public int NhanVienId { get; set; }

    public int? TaiKhoanId { get; set; }

    public string HoTen { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? ChucVu { get; set; }

    public DateOnly? NgayVaoLam { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual TaiKhoan? TaiKhoan { get; set; }
}
