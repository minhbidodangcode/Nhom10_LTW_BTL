using System;
using System.Collections.Generic;

namespace Quanlinhahang.Models;

public partial class KhungGio
{
    public int KhungGioId { get; set; }

    public string TenKhungGio { get; set; } = null!;

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    public virtual ICollection<DatBan> DatBans { get; set; } = new List<DatBan>();
}
