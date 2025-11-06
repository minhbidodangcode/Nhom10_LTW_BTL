using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Quanlinhahang.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Quanlinhahang.Controllers
{
    public class CartController : Controller
    {
        private readonly QuanLyNhaHangContext _context;
        private const string SESSION_CART_KEY = "Cart";

        public CartController(QuanLyNhaHangContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCartItems()
        {
            var session = HttpContext.Session.GetString(SESSION_CART_KEY);
            if (string.IsNullOrEmpty(session)) return new List<CartItem>();
            try
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(session) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(SESSION_CART_KEY, JsonConvert.SerializeObject(cart));
        }

        // POST /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromForm] int monAnId)
        {
            var mon = await _context.MonAns.FindAsync(monAnId);
            if (mon == null) return NotFound(new { success = false, message = "Món không tồn tại" });

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.MonAnId == monAnId);
            if (item == null)
            {
                cart.Add(new CartItem
                {
                    MonAnId = mon.MonAnId,
                    TenMon = mon.TenMon,
                    Gia = mon.DonGia,
                    SoLuong = 1
                });
            }
            else
            {
                item.SoLuong++;
            }

            SaveCart(cart);
            return Json(new { success = true, count = cart.Sum(c => c.SoLuong) });
        }

        // GET /Cart/GetCart
        [HttpGet]
        public IActionResult GetCart()
        {
            var cart = GetCartItems();
            return Json(new { success = true, items = cart });
        }

        // POST /Cart/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            SaveCart(new List<CartItem>());
            return Json(new { success = true });
        }
    }
}
