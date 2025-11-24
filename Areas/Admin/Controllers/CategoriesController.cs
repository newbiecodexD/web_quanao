using System.Linq;
using System.Net;
using System.Web.Mvc;
using web_quanao.Infrastructure.Repositories;
using web_quanao.Infrastructure.UnitOfWork;
using web_quanao.Models;
using web_quanao.ViewModels;

namespace web_quanao.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();
        private readonly IRepository<Category> _repo;
        private readonly IUnitOfWork _uow;
        public CategoriesController()
        {
            _repo = new EfRepository<Category>(_db);
            _uow = new UnitOfWork(_db);
        }

        public ActionResult Index()
        {
            var list = _repo.Query().OrderByDescending(x => x.CategoryId).ToList();
            return View(list);
        }

        public ActionResult Details(int id)
        {
            var item = _repo.Get(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        public ActionResult Create() => View(new CategoryFormViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(CategoryFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var entity = new Category
            {
                Name = vm.Name,
                Description = vm.Description,
                CreatedAt = System.DateTime.UtcNow
            };
            _repo.Add(entity);
            _uow.Complete();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var e = _repo.Get(id);
            if (e == null) return HttpNotFound();
            return View(new CategoryFormViewModel { CategoryId = e.CategoryId, Name = e.Name, Description = e.Description });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(CategoryFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var e = _repo.Get(vm.CategoryId);
            if (e == null) return HttpNotFound();
            e.Name = vm.Name;
            e.Description = vm.Description;
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
