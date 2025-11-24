using System.Web.Mvc;

namespace web_quanao.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private bool IsAdmin() => Session["IsAdmin"] as string == "true";

        public ActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            return View();
        }
    }
}
