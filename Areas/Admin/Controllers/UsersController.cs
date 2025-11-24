using System.Linq;
using System.Web.Mvc;
using web_quanao.Infrastructure.Repositories;
using web_quanao.Infrastructure.UnitOfWork;
using web_quanao.Models;
using web_quanao.ViewModels;

namespace web_quanao.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();
        private readonly IRepository<User> _users;
        public UsersController(){ _users = new EfRepository<User>(_db); }

        // Ch? xem danh sách + s?a vai trò. Không filter, không t?o, không xóa.
        public ActionResult Index()
        {
            var list = _users.Query().OrderBy(u => u.UserId).ToList();
            return View(list);
        }

        public ActionResult Edit(int id)
        {
            var e = _users.Get(id); if (e == null) return HttpNotFound();
            var vm = new UserFormViewModel { UserId = e.UserId, Email = e.Email, Role = e.Role };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(UserFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var e = _users.Get(vm.UserId); if (e == null) return HttpNotFound();
            e.Role = vm.Role; // ch? cho phép ??i vai trò
            _db.SaveChanges();
            TempData["Msg"] = "?ã c?p nh?t vai trò";
            return RedirectToAction("Index");
        }
    }
}
