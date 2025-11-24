using System;
using System.IO;
using System.Linq;
using System.Web;

namespace web_quanao.Services.FileStorage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] AllowedMime = new[] {
            "image/jpeg","image/png","image/gif","image/webp"
        };

        public bool IsAllowedImage(HttpPostedFileBase file, out string reason)
        {
            reason = null;
            if (file == null || file.ContentLength == 0) { reason = "Empty file"; return false; }
            if (file.ContentLength > 8 * 1024 * 1024) { reason = "File too large (>8MB)"; return false; }
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext)) { reason = "Extension not allowed"; return false; }
            if (!AllowedMime.Contains(file.ContentType)) { reason = "MIME not allowed"; return false; }
            return true;
        }

        public string SaveImage(HttpPostedFileBase file, string subFolder = "Products")
        {
            var ext = Path.GetExtension(file.FileName);
            var fileName = Guid.NewGuid().ToString("N") + ext;
            var folder = "~/Uploads/" + (subFolder?.Trim('/') ?? "") ;
            var phys = HttpContext.Current.Server.MapPath(folder);
            if (!Directory.Exists(phys)) Directory.CreateDirectory(phys);
            var path = Path.Combine(phys, fileName);
            file.SaveAs(path);
            return folder + "/" + fileName;
        }
    }
}
