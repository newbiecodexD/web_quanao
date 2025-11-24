using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using web_quanao.Infrastructure.Repositories;
using web_quanao.Infrastructure.UnitOfWork;
using web_quanao.Models;

namespace web_quanao.Areas.Admin.Controllers
{
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

        public ActionResult Index()
        {
            var list = _orders.Query().Include(o => o.User).OrderByDescending(o => o.CreatedAt).ToList();
            return View(list);
        }

        public ActionResult Details(int id)
        {
            var order = _orders.Query().Include(o => o.User).FirstOrDefault(o => o.OrderId == id);
            if (order == null) return HttpNotFound();
            ViewBag.Items = _orderItems.Query().Include(i => i.Product).Include(i => i.Size).Where(i => i.OrderId == id).ToList();
            return View(order);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var order = _orders.Get(id);
            if (order == null) return HttpNotFound();
            order.Status = status;
            _uow.Complete();
            return RedirectToAction("Details", new { id });
        }
    }
}
