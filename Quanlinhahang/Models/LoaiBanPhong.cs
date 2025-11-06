using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

[Table("LoaiBanPhong")]
public partial class LoaiBanPhong
{
    [Key]
    [Column("LoaiBanPhongID")]
    public int LoaiBanPhongId { get; set; }

    [StringLength(100)]
    public string TenLoai { get; set; } = null!;

    [StringLength(255)]
    public string? MoTa { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PhuThu { get; set; }

    [InverseProperty("LoaiBanPhong")]
    public virtual ICollection<BanPhong> BanPhongs { get; set; } = new List<BanPhong>();
}
