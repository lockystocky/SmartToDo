
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

        /// <summary>
        /// Configure OWIN to use OpenIdConnect 
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            ToDoList.Models.Hangfire.ConfigureHangfire(app);
            ToDoList.Models.Hangfire.InitializeJobs();

            //app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            /* app.UseCookieAuthentication(new CookieAuthenticationOptions());
             //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
             app.CreatePerOwinContext(ApplicationDbContext.Create);
             app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
             app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
             /*app.UseOpenIdConnectAuthentication(
             new OpenIdConnectAuthenticationOptions
             {
                 ClientId = clientId,
                 Authority = authority,
                 RedirectUri = redirectUri,
                 PostLogoutRedirectUri = redirectUri,
                 Scope = OpenIdConnectScope.OpenIdProfile,
                 ResponseType = OpenIdConnectResponseType.IdToken,
                 TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters() { ValidateIssuer = false },
                 Notifications = new OpenIdConnectAuthenticationNotifications
                 {
                     AuthenticationFailed = OnAuthenticationFailed
                 }
             }
         );*/
        }

        /// <summary>
        /// Handle failed authentication requests by redirecting the user to the home page with an error in the query string
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        // private Task OnAuthenticationFailed(AuthenticationFailedNotification<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        private Task OnAuthenticationFailed(AuthenticationFailedNotification<Microsoft.IdentityModel.Protocols.OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            context.HandleResponse();
            context.Response.Redirect("/?errormessage=" + context.Exception.Message);
            return Task.FromResult(0);
        }        
    }
}
