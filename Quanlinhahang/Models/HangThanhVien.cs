using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

[Table("HangThanhVien")]
public partial class HangThanhVien
{
    [Key]
    [Column("HangThanhVienID")]
    public int HangThanhVienId { get; set; }

    [StringLength(50)]
    public string TenHang { get; set; } = null!;

    [StringLength(255)]
    public string? MoTa { get; set; }

    public int DiemToiThieu { get; set; }

    public int? DiemToiDa { get; set; }

    [InverseProperty("HangThanhVien")]
    public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();
}
