using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using web_quanao.Models;
using web_quanao.Services.CartDb;

namespace web_quanao.Controllers
{
    public class CartController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();
        private readonly DbCartService _cartDb = new DbCartService();

        private int ResolveUserId()
        {
            var email = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email)) return 0;
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            return user?.UserId ?? 0;
        }

        private string GetOrSetAnonToken()
        {
            const string cookieName = "CART_TOKEN";
            var c = Request.Cookies[cookieName];
            if (c != null && !string.IsNullOrWhiteSpace(c.Value)) return c.Value;
            var token = Guid.NewGuid().ToString("N");
            var newCookie = new HttpCookie(cookieName, token)
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(30)
            };
            Response.Cookies.Add(newCookie);
            return token;
        }

        // GET: /Cart  (hiển thị giỏ từ localStorage phía client; server chỉ cần trả view trống)
        public ActionResult Index()
        {
            return View(); // View đọc localStorage để render.
        }

        // DTO để sync từ localStorage
        public class LocalItemDto
        {
            public int ProductId { get; set; }
            public int? SizeId { get; set; }
            public int Quantity { get; set; }
        }

        // Sync localStorage -> DB (gọi khi user đăng nhập hoặc nhấn thanh toán)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SyncLocal(LocalItemDto[] items)
        {
            if (items == null || items.Length == 0)
                return Json(new { success = false, message = "Không có dữ liệu" });
            var userId = ResolveUserId();
            var token = userId > 0 ? null : GetOrSetAnonToken();
            foreach (var it in items)
            {
                var p = _db.Products.FirstOrDefault(x => x.ProductId == it.ProductId);
                if (p == null) continue;
                var sizeId = it.SizeId ?? _db.ProductSizes.Where(ps => ps.ProductId == p.ProductId).Select(ps => ps.SizeId).FirstOrDefault();
                if (sizeId == 0) sizeId = 1; // fallback nếu không có size
                var qty = it.Quantity > 0 ? it.Quantity : 1;
                _cartDb.AddItem(userId, token, p.ProductId, sizeId, qty, p.Price);
            }
            var count = _cartDb.Count(userId, token);
            return Json(new { success = true, count });
        }

        // Thêm sản phẩm (được gọi từ nút add-to-cart AJAX). Lưu vào DB cart để đồng bộ badge.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int productId, int quantity = 1, int? sizeId = null)
        {
            var p = _db.Products.FirstOrDefault(x => x.ProductId == productId);
            if (p == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            if (quantity <= 0) quantity = 1;
            var userId = ResolveUserId();
            var token = userId > 0 ? null : GetOrSetAnonToken();
            var chosenSize = sizeId ?? _db.ProductSizes.Where(ps => ps.ProductId == p.ProductId).Select(ps => ps.SizeId).FirstOrDefault();
            if (chosenSize == 0) chosenSize = 1; // fallback
            _cartDb.AddItem(userId, token, p.ProductId, chosenSize, quantity, p.Price);
            var count = _cartDb.Count(userId, token);
            return Json(new { success = true, count });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(int cartItemId, int quantity)
        {
            var userId = ResolveUserId();
            var token = userId > 0 ? null : GetOrSetAnonToken();
            _cartDb.Update(userId, token, cartItemId, quantity);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int cartItemId)
        {
            var userId = ResolveUserId();
            var token = userId > 0 ? null : GetOrSetAnonToken();
            _cartDb.Remove(userId, token, cartItemId);
            return Json(new { success = true });
        }
    }
}