using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        async public Task<ActionResult> Calendar()
        {
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                // If there's no token in the session, redirect to Home
                return Redirect("/");
            }

            //string accessToken = User.Claims.FirstOrDefault("access_token")?.Value;

            GraphServiceClient client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);

                        return Task.FromResult(0);
                    }));

           
                var eventResults = await client.Me.Events.Request()
                                    .OrderBy("start/dateTime DESC")
                                    .Select("subject,start,end")
                                    .Top(10)
                                    .GetAsync();
            string str = "";
            foreach (var ev in eventResults)
            {
                str += ev.Subject;
            }

            return View(str);
            
        }

        public async Task<string> GetAccessToken()
        {
            string accessToken = null;

            // Load the app config from web.config
            string appId = ConfigurationManager.AppSettings["ClientId"];
            string appPassword = ConfigurationManager.AppSettings["AppPassword"];
            string redirectUri = ConfigurationManager.AppSettings["RedirectUri"];
            string[] scopes = ConfigurationManager.AppSettings["AppScopes"]
                .Replace(' ', ',').Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            // Get the current user's ID
            string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            WriteLog("userId from calend: " + userId);

            if (!string.IsNullOrEmpty(userId))
            {
                // Get the user's token cache
                SessionTokenCache tokenCache = new SessionTokenCache(userId, HttpContext);

                ConfidentialClientApplication cca = new ConfidentialClientApplication(
                    appId, redirectUri, new ClientCredential(appPassword), tokenCache.GetMsalCacheInstance(), null);

                // Call AcquireTokenSilentAsync, which will return the cached
                // access token if it has not expired. If it has expired, it will
                // handle using the refresh token to get a new one.

                var tt = cca.Users.First();
                AuthenticationResult result = await cca.AcquireTokenSilentAsync(scopes, tt);

                accessToken = result.AccessToken;
            }

            return accessToken;
        }


        /// <summary>
        /// Add user's claims to viewbag
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var claimsPrincipalCurrent = System.Security.Claims.ClaimsPrincipal.Current;
                //You get the user’s first and last name below:
                ViewBag.Name = claimsPrincipalCurrent.FindFirst("name").Value;

                // The 'preferred_username' claim can be used for showing the username
                ViewBag.Username = claimsPrincipalCurrent.FindFirst("preferred_username").Value;

                // The subject claim can be used to uniquely identify the user across the web
                ViewBag.Subject = claimsPrincipalCurrent.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

                // TenantId is the unique Tenant Id - which represents an organization in Azure AD
                ViewBag.TenantId = claimsPrincipalCurrent.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
           
            return View();
        }

        public ActionResult Test()
        {
            return View();
        }

        private void WriteLog(string text)
        {
            using (StreamWriter sw = new StreamWriter("D:\\VS2017_Projects\\todolog.txt", append: true))
            {
                sw.WriteLine(DateTime.Now.ToString() + "   " + text);
            }
        }
    }
}