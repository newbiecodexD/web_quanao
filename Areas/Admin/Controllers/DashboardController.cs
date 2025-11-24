using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using web_quanao.Infrastructure.Repositories;
using web_quanao.Models;

namespace web_quanao.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();
        private readonly EfRepository<User> _users;
        private readonly EfRepository<Order> _orders;
        private readonly EfRepository<Product> _products;

        public DashboardController()
        {
            _users = new EfRepository<User>(_db);
            _orders = new EfRepository<Order>(_db);
            _products = new EfRepository<Product>(_db);
        }

        public ActionResult Index()
        {
            var totalUsers = _users.Query().Count();
            var totalOrders = _orders.Query().Count();
            var totalRevenue = _orders.Query().Sum(o => (decimal?)o.TotalAmount) ?? 0;
            var totalProducts = _products.Query().Count();

            var last7 = Enumerable.Range(0, 7)
                .Select(i => DateTime.UtcNow.Date.AddDays(-i))
                .Select(d => new
                {
                    Date = d,
                    Amount = _orders.Query().Where(o => DbFunctions.TruncateTime(o.CreatedAt) == d).Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .OrderBy(x => x.Date)
                .ToList();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.ChartLabels = string.Join(",", last7.Select(x => "'" + x.Date.ToString("dd/MM") + "'"));
            ViewBag.ChartData = string.Join(",", last7.Select(x => x.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture)));

            var latestOrders = _orders.Query().OrderByDescending(o => o.CreatedAt).Take(10).ToList();
            ViewBag.LatestOrders = latestOrders;
            return View();
        }
    }
}
