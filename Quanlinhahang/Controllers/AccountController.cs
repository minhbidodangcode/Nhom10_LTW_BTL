// Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quanlinhahang.Models; // Namespace chứa TaiKhoan, KhachHang
using System.Security.Cryptography; // Ví dụ đơn giản cho hashing (KHÔNG DÙNG TRONG THỰC TẾ)

[Route("Account")]
public class AccountController : Controller
{
    private readonly QuanLyNhaHangContext _context; // Thay thế bằng tên DbContext của bạn

    public AccountController(QuanLyNhaHangContext context)
    {
        _context = context;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        // 1. Server-side Validation
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, message = string.Join(" | ", errors) });
        }

        // 2. Kiểm tra trùng lặp (Tên đăng nhập, Email, SĐT)
        if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.Username))
        {
            return Conflict(new { success = false, message = "Tên đăng nhập đã tồn tại." });
        }
        if (await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == model.Phone))
        {
            return Conflict(new { success = false, message = "Số điện thoại này đã được đăng ký." });
        }

        // 3. Hash Mật khẩu (Sử dụng PasswordHasher trong dự án thực tế)
        var hashedPassword = HashPassword(model.Password);

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // A. Tạo Tài Khoản (TaiKhoan)
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.Username,
                    MatKhauHash = hashedPassword,
                    Email = model.Email,
                    VaiTro = "Customer",
                    TrangThai = "Hoạt động"
                };
                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync(); // Lưu để lấy TaiKhoanID

                // B. Tạo Khách Hàng (KhachHang) và liên kết Khóa ngoại
                var khachHang = new KhachHang
                {
                    HoTen = model.FullName,
                    Email = model.Email,
                    SoDienThoai = model.Phone,
                    DiaChi = model.Address,
                    DiemTichLuy = 0,
                    HangThanhVienId = 1, // Hạng Vàng mặc định (hoặc ID phù hợp)
                    TaiKhoanId = taiKhoan.TaiKhoanId, // Liên kết Khóa ngoại TaiKhoanID
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

                // TRẢ VỀ LỖI CHI TIẾT ĐỂ DEBUG
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " | Inner Exception: " + ex.InnerException.Message;
                }

                return StatusCode(500, new { success = false, message = errorMessage });
            }
        }
    }

    [HttpGet("Dangki")] 
    public IActionResult Dangki()
    {
        return View();
    }

    // Hàm Hash Mật khẩu giả định (THỰC TẾ: Dùng PasswordHasher)
    private string HashPassword(string password)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}