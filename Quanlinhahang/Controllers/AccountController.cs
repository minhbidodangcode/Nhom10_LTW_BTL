using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quanlinhahang.Models;
using System.Security.Cryptography;
using System.Text;

[Route("Account")]
public class AccountController : Controller
{
    private readonly QuanLyNhaHangContext _context;

    public AccountController(QuanLyNhaHangContext context)
    {
        _context = context;
    }

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
                return Json(new
                {
                    fullName = nhanVien.HoTen,
                    email = nhanVien.TaiKhoan?.Email,
                    phone = nhanVien.SoDienThoai,
                    address = "N/A"
                });
            }
            return NotFound(new { success = false, message = "Không tìm thấy thông tin." });
        }

        return Json(new
        {
            fullName = khachHang.HoTen,
            email = khachHang.Email,
            phone = khachHang.SoDienThoai,
            address = khachHang.DiaChi
        });
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

            return Json(new
            {
                success = true,
                message = "Cập nhật thông tin thành công!",
                newFullName = khachHang.HoTen
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    [HttpGet("History")]
    public IActionResult History()
    {
        return View();
    }

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
        // Tải cả DatBan và HoaDon liên quan
        var query = _context.DatBans
                            .Include(d => d.BanPhong)
                            .Include(d => d.HoaDons)
                            .Where(d => d.KhachHangId == khachHang.KhachHangId);

        // 3. Lọc theo trạng thái (status)
        switch (status.ToLower())
        {
            case "chưa xác nhận":
                // TrangThai trong DatBan: "Chờ xác nhận"
                query = query.Where(d => d.TrangThai == "Chờ xác nhận");
                break;
            case "đã xác nhận":
                query = query.Where(d => d.TrangThai == "Đã xác nhận");
                break;
            case "đang phục vụ":
                query = query.Where(d => d.TrangThai == "Đang phục vụ");
                break;
            case "đã thanh toán":
                // Lọc DatBan có ít nhất một HoaDon có TrangThai = "Đã thanh toán"
                query = query.Where(d => d.HoaDons.Any(h => h.TrangThai == "Đã thanh toán"));
                break;
            case "tất cả":
            default:
                // Không lọc gì thêm
                break;
        }

        // 4. Chọn dữ liệu trả về cho JavaScript
        var historyData = await query
            .OrderByDescending(d => d.NgayDen)
            .Select(d => new
            {
                ngayDen = d.NgayDen.ToString("dd/MM/yyyy"),
                tenBanPhong = d.BanPhong != null ? d.BanPhong.TenBanPhong : "N/A",
                soNguoi = d.SoNguoi,
                trangThaiDatBan = d.TrangThai,
                trangThaiThanhToan = d.HoaDons.OrderByDescending(h => h.NgayLap).Select(h => h.TrangThai).FirstOrDefault()
            })
            .ToListAsync();

        return Json(historyData);
    }

    [HttpGet("Vouchers")]
    public IActionResult Vouchers()
    {
        // Trả về View Views/Account/Vouchers.cshtml
        return View();
    }

    // 2. Action (API) [GET] để LẤY danh sách Voucher của khách hàng
    [HttpGet("GetUserVouchers")]
    public async Task<IActionResult> GetUserVouchers([FromQuery] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { success = false, message = "Không tìm thấy người dùng." });
        }

        // Lấy ID và Điểm tích lũy của khách hàng
        var khachHang = await _context.KhachHangs
                                    .Include(k => k.TaiKhoan)
                                    .Include(k => k.HangThanhVien) // Tải hạng thành viên để hiển thị
                                    .FirstOrDefaultAsync(k => k.TaiKhoan != null && k.TaiKhoan.TenDangNhap == username);

        if (khachHang == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy thông tin khách hàng." });
        }

        // ===============================================
        // GIẢ LẬP LOGIC LẤY VOUCHER (vì bạn chưa có bảng Voucher)
        // ===============================================

        // Giả định 1: Voucher dựa trên Hạng thành viên
        string hangThanhVien = khachHang.HangThanhVien?.TenHang ?? "Thường";
        int diem = khachHang.DiemTichLuy;

        var vouchers = new List<object>();

        // Voucher cố định cho mọi Khách hàng đã đăng nhập
        vouchers.Add(new
        {
            code = "WELCOME10",
            value = 10,
            type = "Phần trăm",
            minOrder = 200000,
            expiry = DateTime.Today.AddDays(30).ToString("dd/MM/yyyy"),
            description = "Giảm 10% cho đơn hàng đầu tiên."
        });

        // Voucher thưởng theo Điểm (Ví dụ)
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

        // Voucher đặc biệt cho hạng Kim cương
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

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}