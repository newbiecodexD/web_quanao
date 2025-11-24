using System.Linq;
using System.Web.Mvc;
using web_quanao.Infrastructure.Repositories;
using web_quanao.Infrastructure.UnitOfWork;
using web_quanao.Models;

namespace web_quanao.Areas.Admin.Controllers
{
    public class ProductImagesController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();
        private readonly IRepository<ProductImage> _repo;
        private readonly IUnitOfWork _uow;
        public ProductImagesController()
        {
            _repo = new EfRepository<ProductImage>(_db);
            _uow = new UnitOfWork(_db);
        }

        public ActionResult Index(int? productId)
        {
            var q = _repo.Query();
            if (productId.HasValue) q = q.Where(x => x.ProductId == productId.Value);
            return View(q.ToList());
        }

        public ActionResult Delete(int id)
        {
            var e = _repo.Get(id);
            if (e == null) return HttpNotFound();
            return View(e);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var e = _repo.Get(id);
            if (e == null) return HttpNotFound();
            _repo.Remove(e);
            _uow.Complete();
            return RedirectToAction("Index");
        }
    }
}
