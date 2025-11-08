using System;
using System.Collections.Generic;

namespace Quanlinhahang.Models;

public partial class MonAn
{
    public int MonAnId { get; set; }

    public int DanhMucId { get; set; }

    public string TenMon { get; set; } = null!;

    public string? MoTa { get; set; }

    public decimal DonGia { get; set; }

    public string? LoaiMon { get; set; }

    public string? HinhAnhUrl { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual DanhMucMon DanhMuc { get; set; } = null!;
}
