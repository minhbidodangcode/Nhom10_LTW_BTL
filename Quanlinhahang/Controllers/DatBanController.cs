using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quanlinhahang.Models;
using System.Security.Cryptography;
using System.Text;

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
            public int? id { get; set; } // MonAnID
            public string? name { get; set; }
            public decimal? price { get; set; }
            public int? qty { get; set; }
        }

        public class BookingRequest
        {
            public string? customerName { get; set; }
            public string? phone { get; set; }
            public string? email { get; set; }
            public string? bookingDate { get; set; }
            public string? timeSlot { get; set; }
            public int? guestCount { get; set; }
            public string? tableType { get; set; }
            public string? note { get; set; }
            public List<BookingItem>? items { get; set; }
        }

        // ----------- POST: /DatBan/Submit (ĐÃ HOÀN THIỆN LOGIC KHÁCH HÀNG) -------------
        [HttpPost("Submit")]
        public async Task<IActionResult> Submit([FromBody] BookingRequest req)
        {
            // 1. Validation cơ bản
            if (req == null || string.IsNullOrWhiteSpace(req.customerName) || string.IsNullOrWhiteSpace(req.phone))
                return Json(new { success = false, message = "Thiếu họ tên, số điện thoại hoặc dữ liệu rỗng" });

            var items = req.items ?? new List<BookingItem>();
            if (items.Count == 0)
                return Json(new { success = false, message = "Giỏ hàng rỗng, không thể đặt bàn." });

            // 2. Resolve FKs
            if (!DateOnly.TryParse(req.bookingDate, out DateOnly bookingDateOnly))
                return Json(new { success = false, message = "Ngày đặt không hợp lệ" });

            int khungGioId = await ResolveKhungGioId(req.timeSlot);
            if (khungGioId == 0)
                return Json(new { success = false, message = "Không tìm thấy khung giờ hợp lệ" });

            // 3. Xử lý Khách hàng (UPSERT LOGIC)
            int khachHangId;
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == req.phone);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (khachHang == null)
                    {
                        // TẠO KHÁCH HÀNG MỚI (nếu số điện thoại chưa từng tồn tại)
                        var newKhachHang = new KhachHang
                        {
                            HoTen = req.customerName,
                            Email = req.email,
                            SoDienThoai = req.phone,
                            DiaChi = null, // Giữ mặc định
                            DiemTichLuy = 0,
                            HangThanhVienId = 1, // Hạng mặc định
                            TaiKhoanId = null,
                            NgayTao = DateTime.Now,
                            TrangThai = "Hoạt động"
                        };
                        _context.KhachHangs.Add(newKhachHang);
                        await _context.SaveChangesAsync();
                        khachHangId = newKhachHang.KhachHangId;
                    }
                    else
                    {
                        khachHangId = khachHang.KhachHangId;
                    }

                    // 4. Tìm Bàn/Phòng ID (Giữ nguyên)
                    int? banPhongId = await ResolveBanPhongId(req.tableType, req.guestCount ?? 1);
                    if (banPhongId == null && !string.IsNullOrWhiteSpace(req.tableType) && req.tableType != "Không yêu cầu")
                        return Json(new { success = false, message = "Không tìm thấy bàn/phòng trống phù hợp với yêu cầu của bạn." });

                    decimal tongTienDuKien = items.Sum(i => (i.price ?? 0) * (i.qty ?? 1));

                    // 5. Tạo DatBan
                    var datBan = new DatBan
                    {
                        KhachHangId = khachHangId, // Gán ID đã tìm hoặc tạo
                        BanPhongId = banPhongId,
                        KhungGioId = khungGioId,
                        NgayDen = bookingDateOnly,
                        SoNguoi = req.guestCount ?? 1,
                        TongTienDuKien = tongTienDuKien,
                        YeuCauDacBiet = req.note,
                        TrangThai = "Chờ xác nhận",
                        NgayTao = DateTime.Now
                    };

                    _context.DatBans.Add(datBan);
                    await _context.SaveChangesAsync();

                    // B. Ghi chú: Nếu bạn cần lưu Chi tiết món ăn (BookingItem) 
                    // bạn cần tạo bảng ChiTietDatBan và thêm logic lưu ở đây.

                    await transaction.CommitAsync();

                    return Json(new
                    {
                        success = true,
                        message = "Đặt bàn thành công! Mã đơn: " + datBan.DatBanId,
                        datBanId = datBan.DatBanId
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // Ghi log ex (ex.Message)
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Lỗi khi lưu đơn đặt bàn: " + ex.Message
                    });
                }
            }
        }

        // ----------- HÀM PHỤ: tìm ID khung giờ theo tên -------------
        private async Task<int> ResolveKhungGioId(string? timeSlot)
        {
            if (string.IsNullOrWhiteSpace(timeSlot)) return 0;

            string key = timeSlot.Trim().ToLower();
            if (key.Contains("trua") || key.Contains("trưa")) key = "Trưa";
            else if (key.Contains("toi") || key.Contains("tối")) key = "Tối";
            else return 0;

            var khungGio = await _context.KhungGios
                .FirstOrDefaultAsync(k => k.TenKhungGio.ToLower() == key.ToLower());

            return khungGio?.KhungGioId ?? 0;
        }

        // ----------- HÀM PHỤ: tìm ID Bàn/Phòng theo loại và sức chứa -------------
        private async Task<int?> ResolveBanPhongId(string? tableType, int guestCount)
        {
            if (string.IsNullOrWhiteSpace(tableType) || tableType == "Không yêu cầu")
                return null;

            // Chuyển đổi giá trị form (Vip, PhongRieng) sang giá trị CSDL (VIP, Phòng riêng)
            string loaiKey = tableType.ToLower() switch
            {
                "vip" => "VIP",
                "phongrieng" => "Phòng riêng",
                _ => "Thường"
            };

            // Tìm LoaiBanPhong
            var loaiBanPhong = await _context.LoaiBanPhongs
                .FirstOrDefaultAsync(l => l.TenLoai == loaiKey);

            if (loaiBanPhong == null) return null;

            // Tìm bàn trống phù hợp
            var banPhong = await _context.BanPhongs
                .Where(b => b.LoaiBanPhongId == loaiBanPhong.LoaiBanPhongId &&
                            b.SucChua >= guestCount &&
                            b.TrangThai == "Trống")
                .OrderBy(b => b.SucChua)
                .FirstOrDefaultAsync();

            return banPhong?.BanPhongId;
        }
    }
}