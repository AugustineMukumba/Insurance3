using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(InsuranceClaim.Startup))]
namespace InsuranceClaim
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
