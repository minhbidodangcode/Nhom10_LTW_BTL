using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quanlinhahang.Models;

namespace Quanlinhahang.Controllers
{
    public class DatBanController : Controller
    {
        private readonly QuanLyNhaHangContext _context;

        public DatBanController(QuanLyNhaHangContext context)
        {
            _context = context;
        }

        // ========== HIỂN THỊ TRANG ĐẶT BÀN ==========
        public IActionResult Index()
        {
            return View();
        }

        // ========== LẤY GIỎ HÀNG TỪ SESSION ==========
        private List<CartItem> GetCartItems()
        {
            var session = HttpContext.Session.GetString("Cart");
            if (session == null)
                return new List<CartItem>();

            return JsonConvert.DeserializeObject<List<CartItem>>(session)!;
        }

        // ========== NHẬN ĐẶT BÀN ==========
        [HttpPost]
        public IActionResult Submit([FromBody] DatBanViewModel model)
        {
            if (model == null || model.Items == null || !model.Items.Any())
                return BadRequest("Giỏ hàng trống.");

            var datBan = new DatBan
            {
                KhungGioId = model.TimeSlot == "Trua" ? 1 : 2,
                NgayDen = DateOnly.FromDateTime(model.BookingDate),
                SoNguoi = model.GuestCount,
                TongTienDuKien = model.Items.Sum(x => (decimal)x.Gia * x.SoLuong),
                YeuCauDacBiet = model.Note,
                TrangThai = "Chờ xác nhận",
                NgayTao = DateTime.Now
            };

            _context.DatBans.Add(datBan);
            _context.SaveChanges();

            return Json(new { success = true });
        }

    }
}
