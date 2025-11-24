using System.Linq;
using System.Web.Mvc;
using web_quanao.Models;

namespace web_quanao.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();

        [HttpGet]
        public ActionResult Index()
        {
            var email = User?.Identity?.Name;
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            var vm = new ManageAccountViewModel
            {
                Email = user?.Email,
                ShippingAddress = user?.ShippingAddress
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ManageAccountViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var email = User?.Identity?.Name;
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return RedirectToAction("Login", "Account");
            user.ShippingAddress = vm.ShippingAddress;
            _db.SaveChanges();
            ViewBag.StatusMessage = "Đã cập nhật thông tin";
            vm.Email = user.Email;
            return View(vm);
        }
    }
}