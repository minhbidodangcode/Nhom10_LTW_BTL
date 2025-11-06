using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

[Table("DatBan")]
public partial class DatBan
{
    [Key]
    [Column("DatBanID")]
    public int DatBanId { get; set; }

    [Column("KhachHangID")]
    public int? KhachHangId { get; set; }

    [Column("BanPhongID")]
    public int? BanPhongId { get; set; }

    [Column("KhungGioID")]
    public int KhungGioId { get; set; }

    public DateOnly NgayDen { get; set; }

    public int SoNguoi { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TongTienDuKien { get; set; }

    public string? YeuCauDacBiet { get; set; }

    [StringLength(30)]
    public string TrangThai { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    [ForeignKey("BanPhongId")]
    [InverseProperty("DatBans")]
    public virtual BanPhong? BanPhong { get; set; }

    [InverseProperty("DatBan")]
    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    [ForeignKey("KhachHangId")]
    [InverseProperty("DatBans")]
    public virtual KhachHang? KhachHang { get; set; }

    [ForeignKey("KhungGioId")]
    [InverseProperty("DatBans")]
    public virtual KhungGio KhungGio { get; set; } = null!;
}
