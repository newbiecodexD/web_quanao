using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using web_quanao.Infrastructure.Repositories;
using web_quanao.Infrastructure.UnitOfWork;
using web_quanao.Models;
using web_quanao.ViewModels;

namespace web_quanao.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ClothingStoreDBEntities _db = new ClothingStoreDBEntities();
        private readonly IRepository<Product> _products;
        private readonly IRepository<Category> _categories;
        private readonly IRepository<ProductImage> _images;
        private readonly IRepository<ProductSize> _productSizes;
        private readonly IRepository<Size> _sizes;
        private readonly IUnitOfWork _uow;

        public ProductsController()
        {
            _products = new EfRepository<Product>(_db);
            _categories = new EfRepository<Category>(_db);
            _images = new EfRepository<ProductImage>(_db);
            _productSizes = new EfRepository<ProductSize>(_db);
            _sizes = new EfRepository<Size>(_db);
            _uow = new UnitOfWork(_db);
        }

        public ActionResult Index()
        {
            var list = _products.Query()
                .Include(x => x.Category)
                .Include(x => x.ProductImages)
                .OrderByDescending(x => x.ProductId)
                .ToList();
            return View(list);
        }

        public ActionResult Details(int id)
        {
            var p = _products.Query()
                .Include(x => x.Category)
                .Include(x => x.ProductImages)
                .Include(x => x.ProductSizes.Select(ps => ps.Size))
                .FirstOrDefault(x => x.ProductId == id);
            if (p == null) return HttpNotFound();
            return View(p);
        }

        public ActionResult Create()
        {
            return View(BuildFormVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(ProductFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormVm(vm));
            }
            if (_products.Query().Any(x => x.Name == vm.Name))
            {
                ModelState.AddModelError("Name", "Tên s?n ph?m ?ã t?n t?i");
                return View(BuildFormVm(vm));
            }
            var entity = new Product
            {
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                CategoryId = vm.CategoryId,
                Gender = vm.Gender,
                CreatedAt = DateTime.UtcNow
            };
            _products.Add(entity);
            _uow.Complete();

            if (vm.Sizes != null)
            {
                foreach (var s in vm.Sizes.Where(s => s.Selected))
                {
                    _productSizes.Add(new ProductSize { ProductId = entity.ProductId, SizeId = s.SizeId, Stock = s.Stock });
                }
            }
            _uow.Complete();

            var files = Request?.Files;
            if (files != null && files.Count > 0)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    var f = files[i];
                    if (f != null && f.ContentLength > 0)
                    {
                        var url = SaveImage(f);
                        _images.Add(new ProductImage { ProductId = entity.ProductId, ImageUrl = url, IsPrimary = i == 0 });
                    }
                }
                _uow.Complete();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var p = _products.Query().Include(x => x.ProductSizes).Include(x => x.ProductImages).Include(x => x.Category).FirstOrDefault(x => x.ProductId == id);
            if (p == null) return HttpNotFound();
            var vm = BuildFormVm(new ProductFormViewModel
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,
                Gender = p.Gender,
                ExistingImages = p.ProductImages.Select(i => i.ImageUrl).ToList(),
                Sizes = _sizes.GetAll().Select(s => new ProductFormViewModel.SizeStockItem
                {
                    SizeId = s.SizeId,
                    SizeName = s.SizeName,
                    Selected = p.ProductSizes.Any(ps => ps.SizeId == s.SizeId),
                    Stock = p.ProductSizes.FirstOrDefault(ps => ps.SizeId == s.SizeId)?.Stock ?? 0
                }).ToList()
            });
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(ProductFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormVm(vm));
            }
            var p = _products.Get(vm.ProductId);
            if (p == null) return HttpNotFound();
            if (_products.Query().Any(x => x.Name == vm.Name && x.ProductId != vm.ProductId))
            {
                ModelState.AddModelError("Name", "Tên s?n ph?m ?ã t?n t?i");
                return View(BuildFormVm(vm));
            }
            p.Name = vm.Name;
            p.Description = vm.Description;
            p.Price = vm.Price;
            p.CategoryId = vm.CategoryId;
            p.Gender = vm.Gender;
            _uow.Complete();

            var existing = _productSizes.Find(x => x.ProductId == p.ProductId).ToList();
            _productSizes.RemoveRange(existing);
            if (vm.Sizes != null)
            {
                foreach (var s in vm.Sizes.Where(s => s.Selected))
                {
                    _productSizes.Add(new ProductSize { ProductId = p.ProductId, SizeId = s.SizeId, Stock = s.Stock });
                }
            }
            _uow.Complete();

            var files = Request?.Files;
            if (files != null && files.Count > 0)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    var f = files[i];
                    if (f != null && f.ContentLength > 0)
                    {
                        var url = SaveImage(f);
                        _images.Add(new ProductImage { ProductId = p.ProductId, ImageUrl = url, IsPrimary = false });
                    }
                }
                _uow.Complete();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var p = _products.Get(id);
            if (p == null) return HttpNotFound();
            return View(p);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var p = _products.Get(id);
            if (p == null) return HttpNotFound();
            var imgs = _images.Find(x => x.ProductId == id).ToList();
            _images.RemoveRange(imgs);
            var sz = _productSizes.Find(x => x.ProductId == id).ToList();
            _productSizes.RemoveRange(sz);
            _products.Remove(p);
            _uow.Complete();
            return RedirectToAction("Index");
        }

        private ProductFormViewModel BuildFormVm(ProductFormViewModel seed = null)
        {
            var vm = seed ?? new ProductFormViewModel();
            vm.Categories = _categories.GetAll().Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name });
            if (vm.Sizes == null)
            {
                vm.Sizes = _sizes.GetAll().Select(s => new ProductFormViewModel.SizeStockItem { SizeId = s.SizeId, SizeName = s.SizeName }).ToList();
            }
            return vm;
        }

        private string SaveImage(HttpPostedFileBase file)
        {
            var fileName = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
            var folder = "~/Uploads/Products";
            var phys = Server.MapPath(folder);
            if (!System.IO.Directory.Exists(phys)) System.IO.Directory.CreateDirectory(phys);
            var path = System.IO.Path.Combine(phys, fileName);
            file.SaveAs(path);
            return folder + "/" + fileName;
        }
    }
}
