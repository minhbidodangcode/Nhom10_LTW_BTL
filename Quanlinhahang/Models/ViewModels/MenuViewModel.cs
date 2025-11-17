using System.Collections.Generic;

namespace Quanlinhahang.Models.ViewModels
{
    public class MenuViewModel
    {
        public List<MonAn> MonAnList { get; set; } = new();  
        public List<DanhMucMon> DanhMucList { get; set; } = new();
        public string? Search { get; set; }
        public int? DanhMucId { get; set; }
    }

}

