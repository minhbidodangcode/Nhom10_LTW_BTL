using System;
using System.Collections.Generic;

namespace Quanlinhahang.Models;

public partial class BanPhong
{
    public int BanPhongId { get; set; }

    public int LoaiBanPhongId { get; set; }

    public string TenBanPhong { get; set; } = null!;

    public int SucChua { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<DatBan> DatBans { get; set; } = new List<DatBan>();

    public virtual LoaiBanPhong LoaiBanPhong { get; set; } = null!;
}
