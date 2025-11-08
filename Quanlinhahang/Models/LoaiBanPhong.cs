using System;
using System.Collections.Generic;

namespace Quanlinhahang.Models;

public partial class LoaiBanPhong
{
    public int LoaiBanPhongId { get; set; }

    public string TenLoai { get; set; } = null!;

    public string? MoTa { get; set; }

    public decimal PhuThu { get; set; }

    public virtual ICollection<BanPhong> BanPhongs { get; set; } = new List<BanPhong>();
}
