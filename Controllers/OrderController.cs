using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using web_quanao.Models;

namespace web_quanao.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        // L?ch s? mua hàng theo tài kho?n
        public ActionResult History()
        {
            var email = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email)) return RedirectToAction("Login", "Account");
            using (var db = new ClothingStoreDBEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email);
                if (user == null) return RedirectToAction("Login", "Account");

                var orders = db.Orders
                    .Include(o => o.OrderItems.Select(i => i.Product))
                    .Include(o => o.OrderItems.Select(i => i.Size))
                    .Where(o => o.UserId == user.UserId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();

                return View(orders);
            }
        }

        public class LocalCartItemDto
        {
            public int ProductId { get; set; }
            public int? SizeId { get; set; }
            public int Quantity { get; set; }
            public decimal? Price { get; set; }
        }

        // Nh?n localStorage cart t? client sau khi ??ng nh?p ?? t?o Order/OrderItems
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateFromLocal(List<LocalCartItemDto> items)
        {
            if (items == null || items.Count == 0)
                return Json(new { success = false, message = "Gi? hàng tr?ng" });

            using (var db = new ClothingStoreDBEntities())
            {
                var email = User?.Identity?.Name;
                var user = db.Users.FirstOrDefault(u => u.Email == email);
                if (user == null) return Json(new { success = false, message = "Không tìm th?y ng??i dùng" });

                var productIds = items.Select(i => i.ProductId).Distinct().ToList();
                var prods = db.Products
                    .Include(p => p.ProductSizes)
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToList();
                if (prods.Count != productIds.Count)
                {
                    return Json(new { success = false, message = "M?t s? s?n ph?m không t?n t?i" });
                }

                // T?o Order
                var order = new Order
                {
                    UserId = user.UserId,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                db.Orders.Add(order);
                db.SaveChanges();

                decimal total = 0m;
                foreach (var it in items)
                {
                    var p = prods.First(x => x.ProductId == it.ProductId);
                    var unit = p.Price; // dùng giá h? th?ng

                    // B?t bu?c SizeId (schema NOT NULL): ch?n size g?i t? client, n?u null thì l?y size ??u tiên c?a s?n ph?m
                    int sizeId;
                    if (it.SizeId.HasValue)
                    {
                        sizeId = it.SizeId.Value;
                    }
                    else
                    {
                        var ps = p.ProductSizes.FirstOrDefault();
                        if (ps == null)
                        {
                            // Không có size nào -> không th? t?o dòng
                            continue;
                        }
                        sizeId = ps.SizeId;
                    }

                    var qty = it.Quantity > 0 ? it.Quantity : 1;
                    total += unit * qty;
                    db.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = p.ProductId,
                        SizeId = sizeId,
                        Quantity = qty,
                        Price = unit
                    });
                }
                order.TotalAmount = total;
                db.SaveChanges();

                return Json(new { success = true, orderId = order.OrderId });
            }
        }
    }
}
