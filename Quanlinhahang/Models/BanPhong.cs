using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

[Table("BanPhong")]
public partial class BanPhong
{
    [Key]
    [Column("BanPhongID")]
    public int BanPhongId { get; set; }

    [Column("LoaiBanPhongID")]
    public int LoaiBanPhongId { get; set; }

    [StringLength(50)]
    public string TenBanPhong { get; set; } = null!;

    public int SucChua { get; set; }

    [StringLength(20)]
    public string TrangThai { get; set; } = null!;

    [InverseProperty("BanPhong")]
    public virtual ICollection<DatBan> DatBans { get; set; } = new List<DatBan>();

    [ForeignKey("LoaiBanPhongId")]
    [InverseProperty("BanPhongs")]
    public virtual LoaiBanPhong LoaiBanPhong { get; set; } = null!;
}
