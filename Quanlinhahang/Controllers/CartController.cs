using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Quanlinhahang.Models;

namespace Quanlinhahang.Controllers
{
    public class CartController : Controller
    {
        private readonly QuanLyNhaHangContext _context;

        public CartController(QuanLyNhaHangContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCartItems()
        {
            var session = HttpContext.Session.GetString("Cart");
            if (session == null)
                return new List<CartItem>();

            return JsonConvert.DeserializeObject<List<CartItem>>(session);
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
        }

        [HttpPost]
        public IActionResult AddToCart(int monAnId)
        {
            var monAn = _context.MonAns.FirstOrDefault(m => m.MonAnId == monAnId);
            if (monAn == null) return NotFound();

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.MonAnId == monAnId);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    MonAnId = monAn.MonAnId,
                    TenMon = monAn.TenMon,
                    Gia = monAn.DonGia,
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

        [HttpGet]
        public IActionResult GetCart()
        {
            var cart = GetCartItems();
            return Json(cart);
        }
    }
}
