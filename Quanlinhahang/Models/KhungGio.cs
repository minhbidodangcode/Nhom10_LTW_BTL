using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

[Table("KhungGio")]
public partial class KhungGio
{
    [Key]
    [Column("KhungGioID")]
    public int KhungGioId { get; set; }

    [StringLength(50)]
    public string TenKhungGio { get; set; } = null!;

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    [InverseProperty("KhungGio")]
    public virtual ICollection<DatBan> DatBans { get; set; } = new List<DatBan>();
}
