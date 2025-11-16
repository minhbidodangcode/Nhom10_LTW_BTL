using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quanlinhahang.Models; // Đảm bảo namespace này chứa Models VÀ ViewModels
using System.Security.Cryptography;
using System.Text;

[Route("Account")]
public class AccountController : Controller
{
    private readonly QuanLyNhaHangContext _context;

    // 1. CONSTRUCTOR
    public AccountController(QuanLyNhaHangContext context)
    {
        _context = context;
    }

    // ==========================================================
    // ACTIONS (GET) ĐỂ MỞ CÁC VIEW (TRANG)
    // ==========================================================

    [HttpGet("Dangki")]
    public IActionResult Dangki()
    {
        return View();
    }

    [HttpGet("Info")]
    public IActionResult Info()
    {
        return View();
    }

    [HttpGet("History")]
    public IActionResult History()
    {
        return View();
    }

    [HttpGet("Vouchers")]
    public IActionResult Vouchers()
    {
        return View();
    }

    // ==========================================================
    // API (POST) CHO CHỨC NĂNG ĐĂNG KÝ
    // ==========================================================
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, message = string.Join(" | ", errors) });
        }
        if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.Username))
        {
            return Conflict(new { success = false, message = "Tên đăng nhập đã tồn tại." });
        }
        if (await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == model.Phone))
        {
            return Conflict(new { success = false, message = "Số điện thoại này đã được đăng ký." });
        }

        var hashedPassword = HashPassword(model.Password);

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.Username,
                    MatKhauHash = hashedPassword,
                    Email = model.Email,
                    VaiTro = "Customer",
                    TrangThai = "Hoạt động"
                };
                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                var khachHang = new KhachHang
                {
                    HoTen = model.FullName,
                    Email = model.Email,
                    SoDienThoai = model.Phone,
                    DiaChi = model.Address,
                    DiemTichLuy = 0,
                    HangThanhVienId = 1,
                    TaiKhoanId = taiKhoan.TaiKhoanId,
                    NgayTao = DateTime.Now,
                    TrangThai = "Hoạt động"
                };
                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Json(new { success = true, message = "Đăng ký tài khoản thành công! 🎉" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                string errorMessage = ex.Message + (ex.InnerException != null ? " | Inner Exception: " + ex.InnerException.Message : "");
                return StatusCode(500, new { success = false, message = errorMessage });
            }
        }
    }

    // ==========================================================
    // API (POST) CHO CHỨC NĂNG ĐĂNG NHẬP
    // ==========================================================
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        var taiKhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(t => t.TenDangNhap == model.Username);

        if (taiKhoan == null)
        {
            return Unauthorized(new { success = false, message = "Sai tài khoản hoặc mật khẩu." });
        }

        var inputPasswordHash = HashPassword(model.Password);
        if (taiKhoan.MatKhauHash != inputPasswordHash)
        {
            return Unauthorized(new { success = false, message = "Sai tài khoản hoặc mật khẩu." });
        }

        string fullName = taiKhoan.TenDangNhap;

        if (taiKhoan.VaiTro == "Customer")
        {
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.TaiKhoanId == taiKhoan.TaiKhoanId);
            if (khachHang != null) fullName = khachHang.HoTen;
        }
        else if (taiKhoan.VaiTro == "Admin" || taiKhoan.VaiTro == "Staff")
        {
            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(nv => nv.TaiKhoanId == taiKhoan.TaiKhoanId);
            if (nhanVien != null) fullName = nhanVien.HoTen;
        }

        var userResponse = new { username = taiKhoan.TenDangNhap, fullName = fullName, role = taiKhoan.VaiTro };
        return Json(new { success = true, user = userResponse });
    }

    // ==========================================================
    // API (POST) CHO CHỨC NĂNG QUÊN MẬT KHẨU
    // ==========================================================

    [HttpPost("CheckUsername")]
    public async Task<IActionResult> CheckUsername([FromBody] CheckUsernameViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username))
        {
            return BadRequest(new { success = false, message = "Vui lòng nhập tên đăng nhập." });
        }

        var taiKhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(t => t.TenDangNhap == model.Username);

        if (taiKhoan == null)
        {
            return Json(new { success = false, message = "Tên đăng nhập không tồn tại." });
        }
        return Json(new { success = true });
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Mật khẩu mới không hợp lệ." });
        }

        var taiKhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(t => t.TenDangNhap == model.Username);

        if (taiKhoan == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy tài khoản để khôi phục." });
        }

        var newHashedPassword = HashPassword(model.NewPassword);
        taiKhoan.MatKhauHash = newHashedPassword;

        try
        {
            _context.TaiKhoans.Update(taiKhoan);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Khôi phục thành công! Vui lòng đăng nhập bằng mật khẩu mới." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Lỗi server khi cập nhật mật khẩu." });
        }
    }

    // ==========================================================
    // API (GET/POST) CHO TRANG THÔNG TIN TÀI KHOẢN
    // ==========================================================

    [HttpGet("GetUserInfo")]
    public async Task<IActionResult> GetUserInfo([FromQuery] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { success = false, message = "Không tìm thấy người dùng." });
        }

        var khachHang = await _context.KhachHangs
                                    .Include(k => k.TaiKhoan)
                                    .FirstOrDefaultAsync(k => k.TaiKhoan != null && k.TaiKhoan.TenDangNhap == username);
        if (khachHang == null)
        {
            var nhanVien = await _context.NhanViens
                                    .Include(n => n.TaiKhoan)
                                    .FirstOrDefaultAsync(n => n.TaiKhoan != null && n.TaiKhoan.TenDangNhap == username);
            if (nhanVien != null)
            {
                return Json(new { fullName = nhanVien.HoTen, email = nhanVien.TaiKhoan?.Email, phone = nhanVien.SoDienThoai, address = "N/A" });
            }
            return NotFound(new { success = false, message = "Không tìm thấy thông tin." });
        }
        return Json(new { fullName = khachHang.HoTen, email = khachHang.Email, phone = khachHang.SoDienThoai, address = khachHang.DiaChi });
    }

    [HttpPost("UpdateUserInfo")]
    public async Task<IActionResult> UpdateUserInfo([FromBody] UpdateInfoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
        }
        var khachHang = await _context.KhachHangs
                                .Include(k => k.TaiKhoan)
                                .FirstOrDefaultAsync(k => k.TaiKhoan != null && k.TaiKhoan.TenDangNhap == model.Username);
        if (khachHang == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy người dùng để cập nhật." });
        }
        khachHang.HoTen = model.FullName;
        khachHang.Email = model.Email;
        khachHang.SoDienThoai = model.Phone;
        khachHang.DiaChi = model.Address;
        try
        {
            _context.KhachHangs.Update(khachHang);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cập nhật thông tin thành công!", newFullName = khachHang.HoTen });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    // ==========================================================
    // API (GET/POST) CHO TRANG LỊCH SỬ (HISTORY)
    // ==========================================================

    [HttpGet("GetHistoryData")]
    public async Task<IActionResult> GetHistoryData([FromQuery] string username, [FromQuery] string status)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { success = false, message = "Không tìm thấy người dùng." });
        }

        // 1. Lấy Khách hàng ID
        var khachHang = await _context.KhachHangs
                                    .Include(k => k.TaiKhoan)
                                    .FirstOrDefaultAsync(k => k.TaiKhoan != null && k.TaiKhoan.TenDangNhap == username);

        if (khachHang == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy thông tin khách hàng." });
        }

        // 2. Truy vấn cơ sở (Query)
        IQueryable<DatBan> query = _context.DatBans
                                        .Include(d => d.BanPhong)
                                        .Include(d => d.HoaDons)
                                            .ThenInclude(h => h.TrangThai)
                                        .Where(d => d.KhachHangId == khachHang.KhachHangId);

        // 3. Lọc theo trạng thái (status)
        int? trangThaiId = null;
        bool filterByDatBanStatus = false;
        bool filterByDatBanHuy = false; // Cờ mới cho trạng thái Hủy

        switch (status.ToLower())
        {
            case "chưa xác nhận":
                filterByDatBanStatus = true;
                break;
            case "đã xác nhận":
                trangThaiId = 2;
                break;
            case "đang phục vụ":
                trangThaiId = 3;
                break;
            case "đã thanh toán":
                trangThaiId = 4;
                break;
            case "đã hủy":
                // SỬA LỖI: Chúng ta cần kiểm tra cả DatBan VÀ HoaDon
                filterByDatBanHuy = true;
                trangThaiId = 5;
                break;
            case "tất cả":
            default:
                break;
        }

        // Áp dụng bộ lọc (ĐÃ SỬA LỖI)
        if (filterByDatBanHuy)
        {
            // Lấy đơn HỦY từ Bảng DatBan (hủy sớm) HOẶC từ Bảng HoaDon (hủy muộn)
            query = query.Where(d =>
                d.TrangThai == "Đã hủy" ||
                d.HoaDons.Any(h => h.TrangThaiId == trangThaiId.Value)
            );
        }
        else if (trangThaiId.HasValue)
        {
            // Lọc các trạng thái Hóa đơn khác
            query = query.Where(d => d.HoaDons.Any(h => h.TrangThaiId == trangThaiId.Value));
        }
        else if (filterByDatBanStatus)
        {
            // Lọc trạng thái "Chờ xác nhận"
            query = query.Where(d => d.TrangThai == "Chờ xác nhận");
        }

        // 4. Chọn dữ liệu trả về (Giữ nguyên)
        var historyData = await query
            .OrderByDescending(d => d.NgayDen)
            .Select(d => new
            {
                datBanId = d.DatBanId,
                ngayDen = d.NgayDen.ToString("dd/MM/yyyy"),
                tenBanPhong = d.BanPhong != null ? d.BanPhong.TenBanPhong : "N/A",
                soNguoi = d.SoNguoi,
                trangThaiDatBan = d.TrangThai,
                trangThaiHoaDon = d.HoaDons
                                    .OrderByDescending(h => h.NgayLap)
                                    .Select(h => h.TrangThai.TenTrangThai)
                                    .FirstOrDefault()
            })
            .ToListAsync();

        return Json(new { success = true, list = historyData });
    }

    [HttpPost("CancelBooking")]
    public async Task<IActionResult> CancelBooking([FromBody] CancelBookingRequest req)
    {
        if (string.IsNullOrEmpty(req.Username) || req.DatBanId <= 0)
        {
            return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        var khachHang = await _context.KhachHangs.Include(k => k.TaiKhoan)
            .FirstOrDefaultAsync(k => k.TaiKhoan != null && k.TaiKhoan.TenDangNhap == req.Username);

        if (khachHang == null)
        {
            return Unauthorized(new { success = false, message = "Không tìm thấy người dùng." });
        }

        var datBan = await _context.DatBans
            .Include(d => d.HoaDons)
            .FirstOrDefaultAsync(d => d.DatBanId == req.DatBanId && d.KhachHangId == khachHang.KhachHangId);

        if (datBan == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy đơn đặt bàn này." });
        }

        if (datBan.TrangThai != "Chờ xác nhận")
        {
            return BadRequest(new { success = false, message = "Không thể hủy đơn. Đơn đã được xác nhận hoặc xử lý." });
        }

        datBan.TrangThai = "Đã hủy";

        var hoaDon = datBan.HoaDons.FirstOrDefault();
        if (hoaDon != null)
        {
            hoaDon.TrangThaiId = 5; // ID 5 = Đã hủy
            _context.HoaDons.Update(hoaDon);
        }

        _context.DatBans.Update(datBan);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Đã hủy đơn thành công." });
    }

    // ==========================================================
    // API (GET) CHO TRANG VOUCHER (LOGIC GIẢ LẬP)
    // ==========================================================

    [HttpGet("GetUserVouchers")]
    public async Task<IActionResult> GetUserVouchers([FromQuery] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { success = false, message = "Không tìm thấy người dùng." });
        }

        var khachHang = await _context.KhachHangs
                                    .Include(k => k.TaiKhoan)
                                    .Include(k => k.HangThanhVien)
                                    .FirstOrDefaultAsync(k => k.TaiKhoan != null && k.TaiKhoan.TenDangNhap == username);

        if (khachHang == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy thông tin khách hàng." });
        }

        string hangThanhVien = khachHang.HangThanhVien?.TenHang ?? "Thường";
        int diem = khachHang.DiemTichLuy;

        var vouchers = new List<object>();
        vouchers.Add(new
        {
            code = "WELCOME10",
            value = 10,
            type = "Phần trăm",
            minOrder = 200000,
            expiry = DateTime.Today.AddDays(30).ToString("dd/MM/yyyy"),
            description = "Giảm 10% cho đơn hàng đầu tiên."
        });
        if (diem >= 1000)
        {
            vouchers.Add(new
            {
                code = "FREE_DRINK",
                value = 1,
                type = "Món ăn",
                minOrder = 0,
                expiry = DateTime.Today.AddMonths(3).ToString("dd/MM/yyyy"),
                description = $"Tặng 1 đồ uống miễn phí (Hạng {hangThanhVien})."
            });
        }
        if (hangThanhVien == "Kim cương")
        {
            vouchers.Add(new
            {
                code = "KIMCUONG20",
                value = 20,
                type = "Phần trăm",
                minOrder = 500000,
                expiry = DateTime.Today.AddYears(1).ToString("dd/MM/yyyy"),
                description = "Giảm 20% đặc biệt cho khách hạng Kim cương."
            });
        }

        return Json(new
        {
            success = true,
            diemTichLuy = diem,
            hangThanhVien = hangThanhVien,
            list = vouchers
        });
    }


    // ==========================================================
    // HÀM HỖ TRỢ (PRIVATE)
    // ==========================================================

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}