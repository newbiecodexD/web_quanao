using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace web_quanao.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private bool IsAdmin() => Session["IsAdmin"] as string == "true";

        // Sử dụng một danh sách tĩnh trong bộ nhớ để demo
        private static readonly List<ProductVm> _products = new List<ProductVm>
        {
            new ProductVm{ ProductID = 1, Name = "Áo thun demo", Cost = 199000, Quantity = 10 },
            new ProductVm{ ProductID = 2, Name = "Quần jean demo", Cost = 499000, Quantity = 5 }
        };

        // ViewModel cho Product, đã được cập nhật theo yêu cầu
        // Lưu ý: "quanlity" được sửa thành "Quantity" theo quy tắc đặt tên chuẩn
        public class ProductVm
        {
            public int ProductID { get; set; }
            public string Name { get; set; }
            public decimal Cost { get; set; }
            public int Quantity { get; set; }
        }

        // GET: Admin/Product (Hiển thị danh sách sản phẩm)
        public ActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            return View(_products.OrderBy(p => p.ProductID));
        }

        // GET: Admin/Product/Create (Hiển thị form tạo mới)
        public ActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            return View();
        }

        // POST: Admin/Product/Create (Xử lý tạo mới)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductVm model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            if (!ModelState.IsValid) return View(model);

            model.ProductID = _products.Any() ? _products.Max(p => p.ProductID) + 1 : 1;
            _products.Add(model);
            TempData["Msg"] = "Đã thêm sản phẩm thành công";
            return RedirectToAction("Index");
        }

        // GET: Admin/Product/Edit/5 (Hiển thị form chỉnh sửa)
        public ActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            var p = _products.FirstOrDefault(x => x.ProductID == id);
            if (p == null) return HttpNotFound();
            return View(p);
        }

        // POST: Admin/Product/Edit/5 (Xử lý cập nhật)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductVm model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            if (!ModelState.IsValid) return View(model);

            var p = _products.FirstOrDefault(x => x.ProductID == model.ProductID);
            if (p == null) return HttpNotFound();

            p.Name = model.Name;
            p.Cost = model.Cost;
            p.Quantity = model.Quantity;

            TempData["Msg"] = "Đã cập nhật sản phẩm";
            return RedirectToAction("Index");
        }

        // GET: Admin/Product/Delete/5 (Hiển thị form xác nhận xóa)
        public ActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            var p = _products.FirstOrDefault(x => x.ProductID == id);
            if (p == null) return HttpNotFound();
            return View(p);
        }

        // POST: Admin/Product/Delete/5 (Xử lý xóa)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account", new { area = "" });
            var p = _products.FirstOrDefault(x => x.ProductID == id);
            if (p != null) _products.Remove(p);

            TempData["Msg"] = "Đã xóa sản phẩm";
            return RedirectToAction("Index");
        }
    }
}
