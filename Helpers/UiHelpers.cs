using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;

namespace web_quanao.Helpers
{
    public static class UiHelpers
    {
        public static IHtmlString Price(this HtmlHelper html, decimal amount)
        {
            var s = string.Format(new CultureInfo("vi-VN"), "{0:C0}", amount);
            return new HtmlString($"<span class=\"text-danger fw-bold\">{HttpUtility.HtmlEncode(s)}</span>");
        }

        public static IHtmlString Image(this HtmlHelper html, string url, string alt = "", string css = "img-fluid")
        {
            if (string.IsNullOrWhiteSpace(url)) return new HtmlString("<span class=\"text-muted\">No image</span>");
            var u = VirtualPathUtility.ToAbsolute(url.StartsWith("~") ? url : "~" + url.TrimStart('/'));
            alt = HttpUtility.HtmlEncode(alt ?? "");
            css = HttpUtility.HtmlEncode(css ?? "");
            return new HtmlString($"<img src=\"{u}\" alt=\"{alt}\" class=\"{css}\"/>");
        }
    }
}
