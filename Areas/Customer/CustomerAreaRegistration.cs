using System.Web.Mvc;

namespace web_quanao.Areas.Customer
{
    public class CustomerAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Customer";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Customer_default",
                url: "Customer/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "web_quanao.Areas.Customer.Controllers" }
            );
        }
    }
}
