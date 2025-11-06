using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Quanlinhahang.Models;

[Table("DanhMucMon")]
public partial class DanhMucMon
{
    [Key]
    [Column("DanhMucID")]
    public int DanhMucId { get; set; }

    [StringLength(100)]
    public string TenDanhMuc { get; set; } = null!;

    [StringLength(255)]
    public string? MoTa { get; set; }

    [InverseProperty("DanhMuc")]
    public virtual ICollection<MonAn> MonAns { get; set; } = new List<MonAn>();
}
