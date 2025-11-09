using System;
using System.Collections.Generic;

namespace Quanlinhahang.Models;

public partial class HoaDon
{
    public int HoaDonId { get; set; }

    public int DatBanId { get; set; }

    public int? TaiKhoanId { get; set; }

    public DateTime NgayLap { get; set; }

    public decimal TongTien { get; set; }

    public decimal GiamGia { get; set; }

    public int DiemCong { get; set; }

    public int DiemSuDung { get; set; }

    public string? HinhThucThanhToan { get; set; }

    public string TrangThai { get; set; } = null!;

    public decimal? Vat { get; set; }

    public string? LoaiDichVu { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual DatBan DatBan { get; set; } = null!;

    public virtual TaiKhoan? TaiKhoan { get; set; }
}
