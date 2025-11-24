using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using web_quanao.Models;

namespace web_quanao.Controllers
{
    public class ProductController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();

        public ActionResult Details(int id)
        {
            var p = _db.Products
                .Include(x => x.ProductImages)
                .Include(x => x.ProductSizes.Select(ps => ps.Size))
                .Include(x => x.Category)
                .FirstOrDefault(x => x.ProductId == id);
            if (p == null) return HttpNotFound();
            return View(p);
        }
    }
}
