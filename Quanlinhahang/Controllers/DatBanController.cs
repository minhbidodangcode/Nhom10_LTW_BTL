using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quanlinhahang.Models;

namespace Quanlinhahang.Controllers
{
    [Route("[controller]")]
    public class DatBanController : Controller
    {
        private readonly QuanLyNhaHangContext _context;

        public DatBanController(QuanLyNhaHangContext context)
        {
            _context = context;
        }

        // ----------- DTOs nhận từ client -------------
        public class BookingItem
        {
            public int? id { get; set; }
            public string? name { get; set; }
            public decimal? price { get; set; }
            public int? qty { get; set; }
        }

        public class BookingRequest
        {
            public string? customerName { get; set; }
            public string? phone { get; set; }
            public string? email { get; set; }
            public string? bookingDate { get; set; } // yyyy-MM-dd
            public string? timeSlot { get; set; }
            public int? guestCount { get; set; }
            public string? tableType { get; set; }
            public string? note { get; set; }
            public List<BookingItem>? items { get; set; }
        }

        // ----------- POST: /DatBan/Submit -------------
        [HttpPost("Submit")]
        public async Task<IActionResult> Submit([FromBody] BookingRequest req)
        {
            if (req == null)
                return Json(new { success = false, message = "Dữ liệu rỗng" });

            if (string.IsNullOrWhiteSpace(req.customerName) || string.IsNullOrWhiteSpace(req.phone))
                return Json(new { success = false, message = "Thiếu họ tên hoặc số điện thoại" });

            var items = req.items ?? new List<BookingItem>();
            if (items.Count == 0)
                return Json(new { success = false, message = "Giỏ hàng rỗng" });

            // === Parse Ngày đặt ===
            DateOnly bookingDateOnly;
            if (!DateOnly.TryParse(req.bookingDate, out bookingDateOnly))
                return Json(new { success = false, message = "Ngày đặt không hợp lệ" });

            // === Khung giờ ===
            int khungGioId = await ResolveKhungGioId(req.timeSlot);
            if (khungGioId == 0)
                return Json(new { success = false, message = "Không tìm thấy khung giờ hợp lệ" });

            // === Tổng tiền dự kiến ===
            decimal tongTien = items.Sum(i => (i.price ?? 0) * (i.qty ?? 1));

            // === Tạo đơn đặt bàn ===
            var datBan = new DatBan
            {
                KhachHangId = null,
                BanPhongId = null,
                KhungGioId = khungGioId,
                NgayDen = bookingDateOnly,
                SoNguoi = req.guestCount ?? 1,
                TongTienDuKien = tongTien,
                YeuCauDacBiet = req.note,
                TrangThai = "Chờ xác nhận",
                NgayTao = DateTime.Now
            };

            try
            {
                _context.DatBans.Add(datBan);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Đặt bàn thành công!",
                    datBanId = datBan.DatBanId
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi khi lưu đơn đặt bàn: " + ex.Message
                });
            }
        }

        // ----------- HÀM PHỤ: tìm ID khung giờ theo tên -------------
        private async Task<int> ResolveKhungGioId(string? timeSlot)
        {
            if (string.IsNullOrWhiteSpace(timeSlot))
                return 0;

            string key = timeSlot.Trim().ToLower();
            if (key.Contains("trua") || key.Contains("trưa")) key = "Trưa";
            else if (key.Contains("toi") || key.Contains("tối")) key = "Tối";

            var khungGio = await _context.KhungGios
                .FirstOrDefaultAsync(k => k.TenKhungGio.ToLower() == key.ToLower());

            return khungGio?.KhungGioId ?? 0;
        }
    }
}
