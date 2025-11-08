using System;
using System.Collections.Generic;

namespace Quanlinhahang.Models;

public partial class DanhMucMon
{
    public int DanhMucId { get; set; }

    public string TenDanhMuc { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<MonAn> MonAns { get; set; } = new List<MonAn>();
}
