using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

[Table("KhachHang")]
public partial class KhachHang
{
    [Key]
    [Column("KhachHangID")]
    public int KhachHangId { get; set; }

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string SoDienThoai { get; set; } = null!;

    [StringLength(255)]
    public string? DiaChi { get; set; }

    public int DiemTichLuy { get; set; }

    [Column("HangThanhVienID")]
    public int? HangThanhVienId { get; set; }

    public DateTime NgayTao { get; set; }

    [StringLength(20)]
    public string TrangThai { get; set; } = null!;

    [InverseProperty("KhachHang")]
    public virtual ICollection<DatBan> DatBans { get; set; } = new List<DatBan>();

    [ForeignKey("HangThanhVienId")]
    [InverseProperty("KhachHangs")]
    public virtual HangThanhVien? HangThanhVien { get; set; }
}
