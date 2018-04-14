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
        // GET: Validation
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult IsFolderNameAvailable(string Name)
        {
            bool folderAvailable = db.Folders.Where(f => f.Name == Name).Count() == 0;

            return Json(folderAvailable, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsFolderNameValid(string Name)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                if (Name.Length < 1 || Name.Contains(invalidChar))
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}