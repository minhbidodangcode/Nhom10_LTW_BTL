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
            public string? username { get; set; }
            public string? bookingDate { get; set; }
            public string? timeSlot { get; set; }
            public int? guestCount { get; set; }
            public string? tableType { get; set; }
            public string? note { get; set; }
            public List<BookingItem>? items { get; set; }
        }

        [HttpPost("Submit")]
        public async Task<IActionResult> Submit([FromBody] BookingRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.username))
                return Unauthorized(new { success = false, message = "Bạn cần đăng nhập để đặt bàn." });

            var khachHang = await _context.KhachHangs
                                        .Include(k => k.TaiKhoan)
                                        .FirstOrDefaultAsync(k => k.TaiKhoan != null && k.TaiKhoan.TenDangNhap == req.username);

            if (khachHang == null)
                return NotFound(new { success = false, message = "Không tìm thấy thông tin khách hàng liên kết với tài khoản này." });

            var items = req.items ?? new List<BookingItem>();
            if (items.Count == 0)
                return Json(new { success = false, message = "Giỏ hàng rỗng, không thể đặt bàn." });

            if (!DateOnly.TryParse(req.bookingDate, out DateOnly bookingDateOnly))
                return Json(new { success = false, message = "Ngày đặt không hợp lệ" });

            int khungGioId = await ResolveKhungGioId(req.timeSlot);
            if (khungGioId == 0)
                return Json(new { success = false, message = "Không tìm thấy khung giờ hợp lệ" });

            int? banPhongId = await ResolveBanPhongId(req.tableType, req.guestCount ?? 1);
            if (banPhongId == null && !string.IsNullOrWhiteSpace(req.tableType) && req.tableType != "Không yêu cầu")
                return Json(new { success = false, message = "Không tìm thấy bàn/phòng trống phù hợp với yêu cầu của bạn." });

            decimal tongTienDuKien = items.Sum(i => (i.price ?? 0) * (i.qty ?? 1));

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var datBan = new DatBan
                    {
                        KhachHangId = khachHang.KhachHangId,
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
                    var hoaDon = new HoaDon
                    {
                        DatBanId = datBan.DatBanId,
                        TaiKhoanId = khachHang.TaiKhoanId,
                        NgayLap = DateTime.Now,
                        TongTien = tongTienDuKien,
                        GiamGia = 0,
                        DiemCong = 0,
                        DiemSuDung = 0,
                        HinhThucThanhToan = null,
                        TrangThaiId = 1,
                        Vat = 0.10m, 
                        LoaiDichVu = "Tại chỗ"
                    };
                    _context.HoaDons.Add(hoaDon);
                    await _context.SaveChangesAsync(); // Lưu để lấy HoaDonID

                    // BƯỚC C: TẠO CHITIETHOADON (Các món ăn)
                    foreach (var item in items)
                    {
                        if (item.id.HasValue && item.id.Value > 0)
                        {
                            var chiTiet = new ChiTietHoaDon
                            {
                                HoaDonId = hoaDon.HoaDonId,
                                MonAnId = item.id.Value,
                                SoLuong = item.qty ?? 1,
                                DonGia = item.price ?? 0, // Lưu giá tại thời điểm đặt
                                ThanhTien = (item.price ?? 0) * (item.qty ?? 1)
                            };
                            _context.ChiTietHoaDons.Add(chiTiet);
                        }
                    }
                    await _context.SaveChangesAsync(); // Lưu tất cả các món ăn

                    // BƯỚC D: Hoàn tất
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

                    // Trả về lỗi chi tiết (InnerException) để dễ gỡ lỗi
                    string errorMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMessage += " | Inner Exception: " + ex.InnerException.Message;
                    }

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Lỗi khi lưu đơn đặt bàn: " + errorMessage
                    });
                }
            }
        }

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

        private async Task<int?> ResolveBanPhongId(string? tableType, int guestCount)
        {
            if (string.IsNullOrWhiteSpace(tableType) || tableType == "Không yêu cầu")
                return null;

            string loaiKey = tableType.ToLower() switch
            {
                "vip" => "VIP",
                "phongrieng" => "Phòng riêng",
                _ => "Thường"
            };

            var loaiBanPhong = await _context.LoaiBanPhongs
                .FirstOrDefaultAsync(l => l.TenLoai == loaiKey);

            if (loaiBanPhong == null) return null;

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