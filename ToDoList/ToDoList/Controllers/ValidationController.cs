using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    public class ValidationController : Controller
    {
        private TasksDbContext db = new TasksDbContext();
        
        [Authorize]
        public JsonResult IsFolderNameAvailable(string Name)
        {
            var context = new ApplicationDbContext();
            var currentUserId = User.Identity.GetUserId();

            bool folderAvailable = db.Folders
                .Where(f => f.Name == Name && f.AppUserId == currentUserId)
                .Count() == 0;

            return Json(folderAvailable, JsonRequestBehavior.AllowGet);
        }        
    }
}