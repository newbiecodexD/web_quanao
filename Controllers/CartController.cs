using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using web_quanao.Models;
using System.Data.SqlClient;
using System.Configuration;

namespace web_quanao.Controllers
{
    // CartController đơn giản giống mẫu: lưu giỏ hàng trong Session, thao tác add/remove và hiển thị icon.
    public class CartController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();
        private const string SessionKey = "Cart";
        private string RawConnStr => ConfigurationManager.ConnectionStrings["ClothingStoreDb"].ConnectionString;

        // Lấy giỏ hàng từ Session (tạo mới nếu null)
        private List<CartItem> GetCartItems()
        {
            var cart = Session[SessionKey] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session[SessionKey] = cart;
            }
            return cart;
        }

        // Hiển thị giỏ hàng
        public ActionResult Index()
        {
            var cart = GetCartItems();
            ViewBag.Total = cart.Sum(i => i.total);
            ViewBag.Count = cart.Sum(i => i.quantity);
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ (GET để dễ dùng link) tương tự mẫu: /Cart/Add/5?backToProducts=1
        public ActionResult Add(int id, string backToProducts = null)
        {
            var cart = GetCartItems();
            var product = _db.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null) return HttpNotFound();

            var existing = cart.FirstOrDefault(c => c.idPro == id);
            if (existing != null)
            {
                existing.quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    idPro = product.ProductId,
                    namePro = product.Name,
                    price = product.Price,
                    quantity = 1
                });
            }

            if (backToProducts == "1")
            {
                TempData["Success"] = "Đã thêm sản phẩm vào giỏ";
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index");
        }

        // AJAX add (POST) returning JSON count
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAjax(int id)
        {
            var cart = GetCartItems();
            var product = _db.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm" });

            var existing = cart.FirstOrDefault(c => c.idPro == id);
            if (existing != null)
            {
                existing.quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    idPro = product.ProductId,
                    namePro = product.Name,
                    price = product.Price,
                    quantity = 1
                });
            }

            return Json(new { success = true, count = cart.Sum(x => x.quantity), total = cart.Sum(x => x.total) });
        }

        // Xóa 1 item khỏi giỏ
        public ActionResult Remove(int id)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.idPro == id);
            if (item != null) cart.Remove(item);
            return RedirectToAction("Index");
        }

        // AJAX remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveAjax(int id)
        {
            var cart = GetCartItems();
            cart.RemoveAll(c => c.idPro == id);
            return Json(new { success = true, count = cart.Sum(x => x.quantity), total = cart.Sum(x => x.total) });
        }

        // AJAX update quantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAjax(int id, int quantity)
        {
            var cart = GetCartItems();
            var existing = cart.FirstOrDefault(c => c.idPro == id);
            if (existing == null) return Json(new { success = false });

            if (quantity < 1) quantity = 1;
            existing.quantity = quantity;
            return Json(new { success = true, count = cart.Sum(x => x.quantity), total = cart.Sum(x => x.total), line = existing.total });
        }

        // Clear cart (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearAjax()
        {
            Session[SessionKey] = new List<CartItem>();
            return Json(new { success = true, count = 0, total = 0 });
        }

        // Cart count endpoint
        [HttpGet]
        public ActionResult Count()
        {
            var cart = GetCartItems();
            return Json(new { count = cart.Sum(x => x.quantity) }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Cart/Checkout - hiển thị xác nhận đơn
        [HttpGet]
        public ActionResult Checkout()
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index");
            }
            var vm = new CheckOutViewModel
            {
                Items = cart,
                Total = cart.Sum(i => i.total),
                DeliveryAddress = GetUserShipping()
            };
            return View(vm);
        }

        private string GetUserShipping()
        {
            var email = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email)) return "";
            var u = _db.Users.FirstOrDefault(x => x.Email == email);
            return u?.ShippingAddress ?? "";
        }

        // POST: Commit order to OrderPro & OrderDetail tables using transaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckoutConfirm(string deliveryAddress, string paymentMethod = "COD")
        {
            var cart = GetCartItems();
            if (!cart.Any()) return Json(new { success = false, message = "Giỏ hàng trống" });
            var email = User?.Identity?.Name;
            var user = !string.IsNullOrWhiteSpace(email) ? _db.Users.FirstOrDefault(x => x.Email == email) : null;
            int? userId = user?.UserId;
            var addr = string.IsNullOrWhiteSpace(deliveryAddress) ? GetUserShipping() : deliveryAddress;
            var prodIds = cart.Select(c => c.idPro).Distinct().ToList();
            var products = _db.Products.Where(p => prodIds.Contains(p.ProductId)).ToList();
            decimal total = 0m;
            foreach (var it in cart)
            {
                var p = products.FirstOrDefault(x => x.ProductId == it.idPro);
                if (p != null) total += p.Price * it.quantity;
            }
            try
            {
                using (var conn = new SqlConnection(RawConnStr))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        // Insert OrderPro (mapping giữ nguyên tên cột theo script bạn đã cung cấp)
                        var cmdOrder = new SqlCommand("INSERT INTO OrderPro (CustomerId, OrderDate, TotalAmount, Status, PaymentMethod, ShippingAddress, Note) VALUES (@cid, @dt, @tot, @st, @pm, @addr, @note); SELECT SCOPE_IDENTITY();", conn, tran);
                        cmdOrder.Parameters.AddWithValue("@cid", (object)userId ?? DBNull.Value);
                        cmdOrder.Parameters.AddWithValue("@dt", DateTime.Now);
                        cmdOrder.Parameters.AddWithValue("@tot", total);
                        cmdOrder.Parameters.AddWithValue("@st", "Pending");
                        cmdOrder.Parameters.AddWithValue("@pm", paymentMethod ?? "COD");
                        cmdOrder.Parameters.AddWithValue("@addr", addr ?? "");
                        cmdOrder.Parameters.AddWithValue("@note", "");
                        int orderId = Convert.ToInt32(cmdOrder.ExecuteScalar());

                        foreach (var it in cart)
                        {
                            var p = products.FirstOrDefault(x => x.ProductId == it.idPro);
                            if (p == null) continue;
                            var cmdDetail = new SqlCommand("INSERT INTO OrderDetail (OrderId, ProductId, ProductName, Quantity, UnitPrice) VALUES (@o,@p,@n,@q,@u)", conn, tran);
                            cmdDetail.Parameters.AddWithValue("@o", orderId);
                            cmdDetail.Parameters.AddWithValue("@p", p.ProductId);
                            cmdDetail.Parameters.AddWithValue("@n", p.Name);
                            cmdDetail.Parameters.AddWithValue("@q", it.quantity);
                            cmdDetail.Parameters.AddWithValue("@u", p.Price);
                            cmdDetail.ExecuteNonQuery();
                        }

                        // Commit
                        tran.Commit();
                        Session[SessionKey] = new List<CartItem>();
                        return Json(new { success = true, orderId });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi tạo đơn hàng: " + ex.Message });
            }
        }

        // Partial icon giỏ hàng (dùng trong layout) -> hiển thị số lượng
        [ChildActionOnly]
        public PartialViewResult CartIcon()
        {
            var cart = GetCartItems();
            ViewBag.CartCount = cart.Sum(x => x.quantity);
            return PartialView("_CartIcon");
        }
    }
}