using System.Web;

namespace web_quanao.Services.FileStorage
{
    public interface IFileStorageService
    {
        // returns virtual url (starting with ~)
        string SaveImage(HttpPostedFileBase file, string subFolder = "Products");
        bool IsAllowedImage(HttpPostedFileBase file, out string reason);
    }
}
