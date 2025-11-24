using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using web_quanao.Infrastructure.Repositories;
using web_quanao.Infrastructure.UnitOfWork;
using web_quanao.Models;

namespace web_quanao.Areas.Admin.Controllers
{
    public class StockManagerController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();
        private readonly IRepository<Product> _products;
        private readonly IRepository<ProductSize> _productSizes;
        private readonly IRepository<Size> _sizes;
        private readonly IUnitOfWork _uow;
        public StockManagerController()
        {
            _products = new EfRepository<Product>(_db);
            _productSizes = new EfRepository<ProductSize>(_db);
            _sizes = new EfRepository<Size>(_db);
            _uow = new UnitOfWork(_db);
        }

        public ActionResult Index(int? productId)
        {
            var products = _products.Query().OrderBy(x => x.Name).ToList();
            ViewBag.Products = new SelectList(products, "ProductId", "Name", productId);
            var q = _productSizes.Query().Include(x => x.Product).Include(x => x.Size);
            if (productId.HasValue) q = q.Where(x => x.ProductId == productId.Value);
            return View(q.ToList());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Update(int productId, int sizeId, int stock)
        {
            var ps = _productSizes.Query().FirstOrDefault(x => x.ProductId == productId && x.SizeId == sizeId);
            if (ps == null)
            {
                ps = new ProductSize { ProductId = productId, SizeId = sizeId, Stock = stock };
                _productSizes.Add(ps);
            }
            else
            {
                ps.Stock = stock;
            }
            _uow.Complete();
            return RedirectToAction("Index", new { productId });
        }
    }
}
