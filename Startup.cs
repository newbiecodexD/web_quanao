using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(web_quanao.Startup))]
namespace web_quanao
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
