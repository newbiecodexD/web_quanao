using System.Linq;
using System.Web.Mvc;
using web_quanao.Infrastructure.Repositories;
using web_quanao.Infrastructure.UnitOfWork;
using web_quanao.Models;

namespace web_quanao.Areas.Admin.Controllers
{
    public class SizesController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();
        private readonly IRepository<Size> _repo;
        private readonly IUnitOfWork _uow;
        public SizesController()
        {
            _repo = new EfRepository<Size>(_db);
            _uow = new UnitOfWork(_db);
        }

        public ActionResult Index() => View(_repo.GetAll());

        public ActionResult Create() => View(new Size());
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Size model)
        {
            if (!ModelState.IsValid) return View(model);
            _repo.Add(model);
            _uow.Complete();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var e = _repo.Get(id);
            if (e == null) return HttpNotFound();
            return View(e);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Size model)
        {
            if (!ModelState.IsValid) return View(model);
            var e = _repo.Get(model.SizeId);
            if (e == null) return HttpNotFound();
            e.SizeName = model.SizeName;
            _uow.Complete();
            return RedirectToAction("Index");
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
