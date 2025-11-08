using System;
using System.Collections.Generic;

namespace Quanlinhahang.Models;

public partial class DatBan
{
    public int DatBanId { get; set; }

    public int? KhachHangId { get; set; }

    public int? BanPhongId { get; set; }

    public int KhungGioId { get; set; }

    public DateOnly NgayDen { get; set; }

    public int SoNguoi { get; set; }

    public decimal? TongTienDuKien { get; set; }

    public string? YeuCauDacBiet { get; set; }

    public string TrangThai { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public virtual BanPhong? BanPhong { get; set; }

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual KhachHang? KhachHang { get; set; }

    public virtual KhungGio KhungGio { get; set; } = null!;
}
