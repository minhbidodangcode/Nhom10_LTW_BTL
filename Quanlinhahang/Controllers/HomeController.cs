using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quanlinhahang.Models;
using System.Diagnostics;
using Quanlinhahang.Models.ViewModels;

namespace Quanlinhahang.Controllers
{
    public class HomeController : Controller
    {
        private readonly QuanLyNhaHangContext _db;

        public HomeController(QuanLyNhaHangContext db)
        {
            _db = db;
        }

        public ActionResult Index()
        {
            return RedirectToAction("GioiThieu");
        }

        public ActionResult GioiThieu()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Menu(string search, int? danhMucId)
        {
            var vm = new MenuViewModel();

            // 1. LOAD DANH MỤC
            var danhMucList = await _db.DanhMucMons.OrderBy(dm => dm.DanhMucId).ToListAsync();

            // Xử lý trùng tên (nếu cần)
            vm.DanhMucList = danhMucList
                .GroupBy(d => d.TenDanhMuc.Trim())
                .Select(g => g.First())
                .ToList();

            // 2. TRUY VẤN MÓN ĂN
            var q = _db.MonAns.AsQueryable();

            // Lọc theo danh mục
            if (danhMucId.HasValue && danhMucId.Value > 0)
            {
                q = q.Where(m => m.DanhMucId == danhMucId.Value);
                vm.DanhMucId = danhMucId;
            }

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(m => EF.Functions.Like(m.TenMon, $"%{s}%")
                              || EF.Functions.Like(m.MoTa, $"%{s}%"));
                vm.Search = search;
            }

            // Chỉ lấy món đang còn bán (Tùy chọn, nhưng nên có)
            // q = q.Where(m => m.TrangThai == "Còn bán");

            vm.MonAnList = await q.OrderBy(m => m.MonAnId).ToListAsync();

            return View(vm);
        }

        // API lưu giỏ hàng vào Session (nếu bạn dùng cách này)
        [HttpPost]
        public IActionResult SaveCart([FromBody] Dictionary<string, CartItem> cart)
        {
            HttpContext.Session.SetString("CartData", JsonConvert.SerializeObject(cart));
            return Ok();
        }

        // Action Đặt bàn: Tải danh sách bàn cho sơ đồ
        [HttpGet]
        public async Task<IActionResult> DatBan()
        {
            // BƯỚC QUAN TRỌNG: Tải dữ liệu bàn từ CSDL
            var danhSachBan = await _db.BanPhongs
                                     .Include(b => b.LoaiBanPhong)
                                     .OrderBy(b => b.BanPhongId)
                                     .ToListAsync();

            var viewModel = new DatBanViewModel
            {
                DanhSachBan = danhSachBan
            };

            // Truyền sang View (nơi có sơ đồ bàn)
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}