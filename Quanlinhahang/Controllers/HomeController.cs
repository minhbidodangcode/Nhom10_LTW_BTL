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
        public ActionResult GioiThieu()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Menu(string search, int? danhMucId)
        {
            var vm = new MenuViewModel();

            // LOAD DANH MỤC (distinct nếu DB có duplicate)
            var danhMucQuery = _db.DanhMucMons.AsQueryable();
            // nếu bạn thấy duplicate names, có thể dùng GroupBy
            var danhMucList = await danhMucQuery
                .OrderBy(dm => dm.DanhMucId)
                .ToListAsync();

            // loại bỏ duplicate tên (nếu thực sự cần)
            vm.DanhMucList = danhMucList
                .GroupBy(d => d.TenDanhMuc.Trim())
                .Select(g => g.First())
                .ToList();

            // TRUY VẤN MÓN ĂN (base query)
            var q = _db.MonAns.AsQueryable();

            if (danhMucId.HasValue && danhMucId.Value > 0)
            {
                q = q.Where(m => m.DanhMucId == danhMucId.Value);
                vm.DanhMucId = danhMucId;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(m => EF.Functions.Like(m.TenMon, $"%{s}%")
                             || EF.Functions.Like(m.MoTa, $"%{s}%"));
                vm.Search = search;
            }

            vm.MonAnList = await q.OrderBy(m => m.MonAnId).ToListAsync();

            return View(vm);
        }
        [HttpPost]
        [HttpPost]
        public IActionResult SaveCart([FromBody] Dictionary<string, CartItem> cart)
        {
            HttpContext.Session.SetString("CartData", JsonConvert.SerializeObject(cart));
            return Ok();
        }

        public async Task<IActionResult> DatBan()
        {
            var viewModel = new DatBanViewModel
            {
                DanhSachBan = await _db.BanPhongs
                                  .Include(b => b.LoaiBanPhong)
                                  .OrderBy(b => b.BanPhongId) 
                                  .ToListAsync()
            };

            return View(viewModel);
        }

        public ActionResult Index()
        {
            return RedirectToAction("GioiThieu");
        }
    }
}
