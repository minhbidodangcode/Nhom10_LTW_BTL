using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

[Table("MonAn")]
public partial class MonAn
{
    [Key]
    [Column("MonAnID")]
    public int MonAnId { get; set; }

    [Column("DanhMucID")]
    public int DanhMucId { get; set; }

    [StringLength(150)]
    public string TenMon { get; set; } = null!;

    public string? MoTa { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DonGia { get; set; }

    [StringLength(50)]
    public string? LoaiMon { get; set; }

    [Column("HinhAnhURL")]
    [StringLength(255)]
    public string? HinhAnhUrl { get; set; }

    [StringLength(20)]
    public string TrangThai { get; set; } = null!;

    [InverseProperty("MonAn")]
    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    [ForeignKey("DanhMucId")]
    [InverseProperty("MonAns")]
    public virtual DanhMucMon DanhMuc { get; set; } = null!;
}
