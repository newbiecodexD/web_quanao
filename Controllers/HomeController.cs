using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using web_quanao.Models;

namespace web_quanao.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new ClothingStoreDBEntities())
            {
                var products = db.Products
                    .Include(p => p.ProductImages)
                    .Select(p => new ProductViewModel
                    {
                        Id = p.ProductId,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Gender = p.Gender,
                        ImageUrl = p.ProductImages.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.ImageId).Select(i => i.ImageUrl).FirstOrDefault()
                    })
                    .ToList();

                var categories = db.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                    .ToList();
                ViewBag.Categories = categories;
                return View(products);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FilterProducts(string gender, int? categoryId, decimal? minPrice, decimal? maxPrice, string q)
        {
            using (var db = new ClothingStoreDBEntities())
            {
                var query = db.Products.Include(p => p.ProductImages).AsQueryable();
                if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Name.Contains(q) || p.Description.Contains(q));
                if (!string.IsNullOrWhiteSpace(gender) && !string.Equals(gender, "Any", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.Gender == gender);
                }
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }
                if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
                if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

                var list = query
                    .OrderByDescending(p => p.ProductId)
                    .Select(p => new ProductViewModel
                    {
                        Id = p.ProductId,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Gender = p.Gender,
                        ImageUrl = p.ProductImages.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.ImageId).Select(i => i.ImageUrl).FirstOrDefault()
                    }).ToList();

                return PartialView("_ProductList", list);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        public ActionResult MenClothes() { return View(); }
        public ActionResult WomenClothes() { return View(); }
        public ActionResult KidsClothes() { return View(); }
        public ActionResult Collections() { return View(); }
    }
}