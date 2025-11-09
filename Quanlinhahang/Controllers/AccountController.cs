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

    // [POST] /Account/Login
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
    // === BẮT ĐẦU PHẦN SỬA LỖI CHO TRANG INFO.CSHTML ===
    // ==========================================================

    // (Trong file Controllers/AccountController.cs)

    // [GET] /Account/GetUserInfo (ĐÃ SỬA LỖI)
    [HttpGet("GetUserInfo")]
    public async Task<IActionResult> GetUserInfo([FromQuery] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { success = false, message = "Không tìm thấy người dùng." });
        }

        // SỬA LỖI: Thêm .Include(k => k.TaiKhoan)
        var khachHang = await _context.KhachHangs
                                    .Include(k => k.TaiKhoan) // <-- SỬA Ở ĐÂY
                                    .FirstOrDefaultAsync(k => k.TaiKhoan != null && k.TaiKhoan.TenDangNhap == username);

        if (khachHang == null)
        {
            var nhanVien = await _context.NhanViens
                                    .Include(n => n.TaiKhoan) // <-- SỬA Ở ĐÂY
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

        // SỬA LỖI: Thêm .Include(k => k.TaiKhoan)
        var khachHang = await _context.KhachHangs
                                .Include(k => k.TaiKhoan) // <-- SỬA Ở ĐÂY
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

    // Hàm Hash Mật khẩu
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}