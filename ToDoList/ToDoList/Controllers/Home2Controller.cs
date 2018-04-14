using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

namespace ToDoList.Controllers
{
    [Authorize]
    public class Home2Controller : Controller
    {
        private void WriteLog(string text)
        {
            using (StreamWriter sw = new StreamWriter("D:\\VS2017_Projects\\todolog.txt", append: true))
            {
                sw.WriteLine(DateTime.Now.ToString() + "   " + text);
            }
        }
        /// <summary>
        /// Send an OpenID Connect sign-in request.
        /// Alternatively, you can just decorate the SignIn method with the [Authorize] attribute
        /// </summary>
        public void SignIn()
        {
            WriteLog("In sign in");
            if (!Request.IsAuthenticated)
            {
                WriteLog("Not auth");
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/claims/index" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
                WriteLog("End");
            }
        }

        /// <summary>
        /// Send an OpenID Connect sign-out request.
        /// </summary>
        public void SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(
                    OpenIdConnectAuthenticationDefaults.AuthenticationType,
                    CookieAuthenticationDefaults.AuthenticationType);
        }

        // GET: Home2
        public ActionResult Index()
        {
            WriteLog("In index");
            return View();
        }
    }
}