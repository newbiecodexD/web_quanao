using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using web_quanao.Infrastructure.Repositories;
using web_quanao.Infrastructure.UnitOfWork;
using web_quanao.Models;
using System.Collections.Generic;

namespace web_quanao.Areas.Admin.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();
        private readonly IRepository<Order> _orders;
        private readonly IRepository<OrderItem> _orderItems;
        private readonly IRepository<User> _users;
        private readonly IRepository<Product> _products;
        private readonly IUnitOfWork _uow;

        public OrdersController()
        {
            _orders = new EfRepository<Order>(_db);
            _orderItems = new EfRepository<OrderItem>(_db);
            _users = new EfRepository<User>(_db);
            _products = new EfRepository<Product>(_db);
            _uow = new UnitOfWork(_db);
        }

        private bool IsAdmin() { return (Session["IsAdmin"] as string) == "true"; }

        public class AdminOrderRow
        {
            public int OrderId { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; }
            public string CustomerName { get; set; }
        }
        public class AdminOrderItemRow
        {
            public int DetailId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal LineTotal => UnitPrice * Quantity;
        }

        public ActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            var data = _db.Database.SqlQuery<AdminOrderRow>("SELECT o.OrderId, o.OrderDate, o.TotalAmount, o.Status, u.FullName AS CustomerName FROM OrderPro o LEFT JOIN Users u ON o.CustomerId = u.UserId ORDER BY o.OrderDate DESC").ToList();
            return View(data);
        }

        public ActionResult Details(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            var order = _db.Database.SqlQuery<AdminOrderRow>("SELECT o.OrderId, o.OrderDate, o.TotalAmount, o.Status, u.FullName AS CustomerName FROM OrderPro o LEFT JOIN Users u ON o.CustomerId = u.UserId WHERE o.OrderId = @p0", id).FirstOrDefault();
            if (order == null) return HttpNotFound();
            var items = _db.Database.SqlQuery<AdminOrderItemRow>("SELECT d.DetailId, d.ProductId, d.ProductName, d.Quantity, d.UnitPrice FROM OrderDetail d WHERE d.OrderId = @p0", id).ToList();
            ViewBag.Items = items;
            return View(order);
        }

        public ActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            var order = _db.Database.SqlQuery<AdminOrderRow>("SELECT o.OrderId, o.OrderDate, o.TotalAmount, o.Status, u.FullName AS CustomerName FROM OrderPro o LEFT JOIN Users u ON o.CustomerId = u.UserId WHERE o.OrderId = @p0", id).FirstOrDefault();
            if (order == null) return HttpNotFound();
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            _db.Database.ExecuteSqlCommand("DELETE FROM OrderDetail WHERE OrderId=@p0", id);
            _db.Database.ExecuteSqlCommand("DELETE FROM OrderPro WHERE OrderId=@p0", id);
            TempData["Msg"] = "?ã xóa ??n hàng";
            return RedirectToAction("Index");
        }
    }
}
