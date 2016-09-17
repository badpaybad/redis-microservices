using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RedisMicroservices.SampleWeb.Startup))]
namespace RedisMicroservices.SampleWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
