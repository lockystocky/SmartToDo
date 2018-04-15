
using Microsoft.Owin;
using Owin;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Notifications;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using ToDoList.Models;
using System.IO;
using Microsoft.AspNet.Identity;
using Hangfire;
using ToDoList.Models;

[assembly: OwinStartupAttribute(typeof(ToDoList.Startup))]
namespace ToDoList
{
    public partial class Startup
    {
        string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];
        string redirectUri = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];
        static string tenant = System.Configuration.ConfigurationManager.AppSettings["Tenant"];
        string authority = String.Format(System.Globalization.CultureInfo.InvariantCulture, System.Configuration.ConfigurationManager.AppSettings["Authority"], tenant);

        
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            ToDoList.Models.Hangfire.ConfigureHangfire(app);
            ToDoList.Models.Hangfire.InitializeJobs();            
        }
    }
}
