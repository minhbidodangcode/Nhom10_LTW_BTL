using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

[PrimaryKey("HoaDonId", "MonAnId")]
[Table("ChiTietHoaDon")]
public partial class ChiTietHoaDon
{
    [Key]
    [Column("HoaDonID")]
    public int HoaDonId { get; set; }

    [Key]
    [Column("MonAnID")]
    public int MonAnId { get; set; }

    public int SoLuong { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DonGia { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ThanhTien { get; set; }

    [ForeignKey("HoaDonId")]
    [InverseProperty("ChiTietHoaDons")]
    public virtual HoaDon HoaDon { get; set; } = null!;

    [ForeignKey("MonAnId")]
    [InverseProperty("ChiTietHoaDons")]
    public virtual MonAn MonAn { get; set; } = null!;
}
