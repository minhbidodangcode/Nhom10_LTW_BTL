using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

[Table("HoaDon")]
public partial class HoaDon
{
    [Key]
    [Column("HoaDonID")]
    public int HoaDonId { get; set; }

    [Column("DatBanID")]
    public int DatBanId { get; set; }

    [Column("TaiKhoanID")]
    public int? TaiKhoanId { get; set; }

    public DateTime NgayLap { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TongTien { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal GiamGia { get; set; }

    public int DiemCong { get; set; }

    public int DiemSuDung { get; set; }

    [StringLength(50)]
    public string? HinhThucThanhToan { get; set; }

    [StringLength(30)]
    public string TrangThaiThanhToan { get; set; } = null!;

    [InverseProperty("HoaDon")]
    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    [ForeignKey("DatBanId")]
    [InverseProperty("HoaDons")]
    public virtual DatBan DatBan { get; set; } = null!;

    [ForeignKey("TaiKhoanId")]
    [InverseProperty("HoaDons")]
    public virtual TaiKhoan? TaiKhoan { get; set; }
}
