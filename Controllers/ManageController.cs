using System.Linq;
using System.Web.Mvc;
using web_quanao.Models;
using Microsoft.AspNet.Identity;

namespace web_quanao.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();

        private User GetCurrentUser()
        {
            var email = User?.Identity?.Name;
            return string.IsNullOrWhiteSpace(email) ? null : _db.Users.FirstOrDefault(u => u.Email == email);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");
            ViewBag.AccountVm = new AccountInfoViewModel { Email = user.Email, ShippingAddress = user.ShippingAddress };
            if (TempData["Msg"] != null) ViewBag.Msg = TempData["Msg"];
            if (TempData["Err"] != null) ViewBag.Err = TempData["Err"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAccountInfo(AccountInfoViewModel vm)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid) { TempData["Err"] = "Dữ liệu không hợp lệ."; return RedirectToAction("Index"); }
            user.ShippingAddress = vm.ShippingAddress; _db.SaveChanges(); TempData["Msg"] = "Đã cập nhật địa chỉ giao hàng."; return RedirectToAction("Index");
        }

        public class ChangePasswordPostModel
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmNewPassword { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordPostModel vm)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Account");
            if (string.IsNullOrWhiteSpace(vm.CurrentPassword) || string.IsNullOrWhiteSpace(vm.NewPassword) || string.IsNullOrWhiteSpace(vm.ConfirmNewPassword))
            { TempData["Err"] = "Điền đầy đủ các trường mật khẩu."; return RedirectToAction("Index"); }
            if (vm.NewPassword != vm.ConfirmNewPassword) { TempData["Err"] = "Xác nhận mật khẩu không khớp."; return RedirectToAction("Index"); }
            var hasher = new PasswordHasher();
            var verify = hasher.VerifyHashedPassword(user.PasswordHash ?? string.Empty, vm.CurrentPassword);
            if (verify != PasswordVerificationResult.Success) { TempData["Err"] = "Mật khẩu hiện tại sai."; return RedirectToAction("Index"); }
            if (vm.CurrentPassword == vm.NewPassword) { TempData["Err"] = "Mật khẩu mới phải khác mật khẩu hiện tại."; return RedirectToAction("Index"); }
            user.PasswordHash = hasher.HashPassword(vm.NewPassword); _db.SaveChanges(); TempData["Msg"] = "Đổi mật khẩu thành công."; return RedirectToAction("Index");
        }
    }
}