using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using web_quanao.Models;
using System.Configuration;
using System.Data.SqlClient;

namespace web_quanao.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private string RawConnStr => ConfigurationManager.ConnectionStrings["ClothingStoreDb"].ConnectionString;

        public class OrderHistoryItemVm
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal LineTotal => UnitPrice * Quantity;
        }
        public class OrderHistoryVm
        {
            public int OrderId { get; set; }
            public DateTime OrderDate { get; set; }
            public string Status { get; set; }
            public decimal TotalAmount { get; set; }
            public List<OrderHistoryItemVm> Items { get; set; } = new List<OrderHistoryItemVm>();
        }

        // L?ch s? mua hàng theo tài kho?n: g?p c? Order (EF) và OrderPro (ADO.NET)
        public ActionResult History()
        {
            var email = User?.Identity?.Name; if(string.IsNullOrWhiteSpace(email)) return RedirectToAction("Login","Account");
            using(var db = new ClothingStoreDBEntities())
            {
                var user = db.Users.FirstOrDefault(u=>u.Email==email); if(user==null) return RedirectToAction("Login","Account");
                var list = new List<OrderHistoryVm>();

                // EF Orders
                var efOrders = db.Orders
                    .Include(o=>o.OrderItems.Select(i=>i.Product))
                    .Where(o=>o.UserId==user.UserId)
                    .ToList();
                foreach(var o in efOrders)
                {
                    var vm = new OrderHistoryVm{ OrderId = o.OrderId, OrderDate = o.CreatedAt ?? DateTime.MinValue, Status = o.Status, TotalAmount = o.TotalAmount ?? 0m };
                    foreach(var it in o.OrderItems)
                    {
                        vm.Items.Add(new OrderHistoryItemVm{ ProductName = it.Product?.Name ?? "S?n ph?m", Quantity = it.Quantity, UnitPrice = it.Price });
                    }
                    list.Add(vm);
                }

                // OrderPro via ADO.NET
                using(var conn = new SqlConnection(RawConnStr))
                {
                    conn.Open();
                    var ordersCmd = new SqlCommand("SELECT OrderId, OrderDate, TotalAmount, Status FROM OrderPro WHERE CustomerId = @uid ORDER BY OrderDate DESC", conn);
                    ordersCmd.Parameters.AddWithValue("@uid", user.UserId);
                    var orders = new List<OrderHistoryVm>();
                    using(var r = ordersCmd.ExecuteReader())
                    {
                        while(r.Read())
                        {
                            orders.Add(new OrderHistoryVm{
                                OrderId = Convert.ToInt32(r["OrderId"]),
                                OrderDate = Convert.ToDateTime(r["OrderDate"]),
                                Status = Convert.ToString(r["Status"]) ?? "",
                                TotalAmount = Convert.ToDecimal(r["TotalAmount"])
                            });
                        }
                    }
                    if(orders.Count>0)
                    {
                        var ids = string.Join(",", orders.Select(x=>x.OrderId));
                        var detailsCmd = new SqlCommand($"SELECT OrderId, ProductName, Quantity, UnitPrice FROM OrderDetail WHERE OrderId IN ({ids})", conn);
                        using(var r2 = detailsCmd.ExecuteReader())
                        {
                            while(r2.Read())
                            {
                                int oid = Convert.ToInt32(r2["OrderId"]);
                                var order = orders.FirstOrDefault(x=>x.OrderId==oid); if(order==null) continue;
                                order.Items.Add(new OrderHistoryItemVm{
                                    ProductName = Convert.ToString(r2["ProductName"]) ?? "S?n ph?m",
                                    Quantity = Convert.ToInt32(r2["Quantity"]),
                                    UnitPrice = Convert.ToDecimal(r2["UnitPrice"]) });
                            }
                        }
                        list.AddRange(orders);
                    }
                }

                var result = list.OrderByDescending(x=>x.OrderDate).ToList();
                return View(result);
            }
        }

        public class LocalCartItemDto{ public int ProductId { get; set; } public int? SizeId { get; set; } public int Quantity { get; set; } public decimal? Price { get; set; } }
        [HttpPost][ValidateAntiForgeryToken]
        public ActionResult CreateFromLocal(List<LocalCartItemDto> items)
        {
            if(items==null || items.Count==0) return Json(new{success=false,message="Gi? hàng tr?ng"});
            using(var db = new ClothingStoreDBEntities())
            {
                var email = User?.Identity?.Name; var user = db.Users.FirstOrDefault(u=>u.Email==email); if(user==null) return Json(new{success=false,message="Không tìm th?y ng??i dùng"});
                var productIds = items.Select(i=>i.ProductId).Distinct().ToList(); var prods = db.Products.Include(p=>p.ProductSizes).Where(p=>productIds.Contains(p.ProductId)).ToList(); if(prods.Count!=productIds.Count) return Json(new{success=false,message="M?t s? s?n ph?m không t?n t?i"});
                var order = new Order{ UserId=user.UserId, Status="Pending", CreatedAt=DateTime.UtcNow }; db.Orders.Add(order); db.SaveChanges(); decimal total=0m;
                foreach(var it in items){ var p = prods.First(x=>x.ProductId==it.ProductId); var unit = p.Price; int sizeId = it.SizeId ?? p.ProductSizes.Select(ps=>ps.SizeId).FirstOrDefault(); if(sizeId==0) continue; var qty = it.Quantity>0? it.Quantity:1; total += unit*qty; db.OrderItems.Add(new OrderItem{ OrderId=order.OrderId, ProductId=p.ProductId, SizeId=sizeId, Quantity=qty, Price=unit }); }
                order.TotalAmount = total; db.SaveChanges(); return Json(new{success=true,orderId=order.OrderId});
            }
        }
    }
}
