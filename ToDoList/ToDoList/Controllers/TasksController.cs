using Microsoft.AspNet.Identity;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class TasksController : Controller
    {
        private TasksDbContext db = new TasksDbContext();

        [Route("tasks/download/{taskId}")]
        public ActionResult DownloadTaskAttachment(Guid taskId)
        {
            TaskToDo taskToDo = db.TaskToDoes.Find(taskId);

            if (taskToDo == null)
                return HttpNotFound();
            string filePath = taskToDo.PathToAttachedFile;            
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string fileName = Path.GetFileName(filePath);

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        // GET: Tasks
       /* public ActionResult Index()
        {
            var tasks = db.TaskToDoes.ToList();
            for(int i = 0; i < tasks.Count; i++)
            {
                Regex regex = new Regex(@"#\w+");
                foreach (Match match in regex.Matches(tasks[i].Description))
                {
                    string tag = match.Value;
                    string href = Url.Action("Hashtag", "Tasks") + "\\" + tag.Substring(1);
                    tasks[i].Description = tasks[i].Description.Replace(tag, String.Format("<a href=\"{0}\">{1}</a>", href, tag));
                     }
            }
            return View(tasks);
        }*/

        private void InsertHashtagReferences(List<TaskToDo> tasks)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                Regex regex = new Regex(@"#\w+");
                foreach (Match match in regex.Matches(tasks[i].Description))
                {
                    string tag = match.Value;
                    string href = "/tasks/hashtag/" + tag.Substring(1);
                    tasks[i].Description = tasks[i].Description.Replace(tag, String.Format("<a href=\"{0}\">{1}</a>", href, tag));
                }
            }
        }
              
        [HttpPost]
        public ActionResult ChangeFavoriteValue(Guid? taskId)
        {
            var task = db.TaskToDoes.Find(taskId);
            if (task == null)
                return HttpNotFound();

            task.IsFavorite = !task.IsFavorite;
            db.SaveChanges();
            return Json(new { success = "Valid" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChangeDoneValue(Guid? taskId)
        {
            var task = db.TaskToDoes.Find(taskId);
            if (task == null)
                return HttpNotFound();

            task.IsDone = !task.IsDone;
            db.SaveChanges();
            return Json(new { success = "Valid" }, JsonRequestBehavior.AllowGet);
        }

        [Route("tasks/folder/{folderName}")]
        public ActionResult Folder(string folderName)
        {
            Models.Folder folder = db.Folders.Where(f => f.Name == folderName).FirstOrDefault();

            if (folder == null)
                return HttpNotFound();

            var tasks = db.TaskToDoes.Where(task => task.Folder.Name == folderName).ToList();
            
            InsertHashtagReferences(tasks);
            return View(tasks);
        }

        [Authorize]
        public ActionResult IndexWithFolders()
        {
            var context = new ApplicationDbContext();
            var currentUserId = User.Identity.GetUserId();

            TasksAndFoldersViewModel tasksAndFoldersForAuth = new TasksAndFoldersViewModel();

            var tasksForAuth = db.TaskToDoes
                .Where(task => task.Folder.Name == "Default" && task.AppUserId == currentUserId)
                .ToList();

            InsertHashtagReferences(tasksForAuth);
            tasksAndFoldersForAuth.Tasks = tasksForAuth;

            var foldersForAuth = db.Folders
                .Where(folder => folder.Name != "Default" && folder.AppUserId == currentUserId)
                .ToList();

            tasksAndFoldersForAuth.Folders = foldersForAuth;

            return View(tasksAndFoldersForAuth);
        }
        
        [Authorize]
        [Route("tasks/hashtag/{tag}")]
        public ActionResult Hashtag(string tag)
        {
            var context = new ApplicationDbContext();
            var currentUserId = User.Identity.GetUserId();

            var hashtag = db.Hashtags
                .Where(h => h.Name == tag)
                .FirstOrDefault();

            if (hashtag == null)
                return HttpNotFound();

            var tasksWithHashtag = hashtag.TasksWithHashtag
                .Where(t => t.AppUserId == currentUserId)
                .ToList();

            InsertHashtagReferences(tasksWithHashtag);

            return View(tasksWithHashtag);
        }

        
        [Authorize]
        public ActionResult CreateWithFile()
        {
            var context = new ApplicationDbContext();
            var currentUserId = User.Identity.GetUserId();

            TaskViewModel taskViewModel = new TaskViewModel();
            var folders = db.Folders.Where(f => f.AppUserId == currentUserId).ToList();
            bool defaultFolderExists = folders.Where(f => f.Name == "Default").Count() > 0;
            if (!defaultFolderExists)
            {
                Models.Folder defaultFolder =
                    new Models.Folder { Name = "Default", Id = Guid.NewGuid(), AppUserId = currentUserId };
                db.Folders.Add(defaultFolder);
                db.SaveChanges();
                folders.Add(defaultFolder);
            }
            taskViewModel.AvailableFolders = new SelectList(folders, "Id", "Name");
            return View(taskViewModel);
        }

        [HttpPost]
        [Authorize]
        public ActionResult CreateWithFile(TaskViewModel task, HttpPostedFileBase file)
        {

            if (ModelState.IsValid)
            {
                var context = new ApplicationDbContext();
                var currentUserId = User.Identity.GetUserId();
                               
                string path = "";
                if (file != null && file.ContentLength > 0)
                {
                    path = Path.Combine(Server.MapPath("~/UploadedFiles"), Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                }
                    
                TaskToDo taskToDo = task.TaskToDo;
                taskToDo.Id = Guid.NewGuid();
                Models.Folder folder = db.Folders.Find(task.FolderId);

                if (folder == null)
                    return View(task);

                taskToDo.Folder = folder;
                taskToDo.StartDate = DateTime.Now;
                taskToDo.AppUserId = currentUserId;
                taskToDo.PathToAttachedFile = path;

                db.TaskToDoes.Add(taskToDo);

                if (taskToDo.Description.Contains("#"))
                    ManageHashtags(taskToDo);

                db.SaveChanges();
                return RedirectToAction("IndexWithFolders");
            }

            return View(task);
        }

        [Authorize]
        public ActionResult CreateFolder()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateFolder(Models.Folder folder)
        {
            if (ModelState.IsValid)
            {
                folder.Id = Guid.NewGuid();
                var context = new ApplicationDbContext();
                var currentUserId = User.Identity.GetUserId();
                folder.AppUserId = currentUserId;
                db.Folders.Add(folder);
                db.SaveChanges();

                return RedirectToAction("IndexWithFolders");
            }

            return View(folder);
        }

        public ActionResult GetAllHashtags()
        {
            var tags = db.Hashtags.ToList();
            return View(tags);
        }


        private void WriteLog(string text)
        {
            using (StreamWriter sw = new StreamWriter("D:\\VS2017_Projects\\todolog.txt", append: true))
            {
                sw.WriteLine(DateTime.Now.ToString()+ "   " + text);
            }
        }

        private void ManageHashtags(TaskToDo task)
        {
            Regex regex = new Regex(@"#\w+");
            foreach (Match match in regex.Matches(task.Description))
            {
                string tag = match.Value.Substring(1).ToLower();
                WriteLog(tag);

               Hashtag hashtag = db.Hashtags.FirstOrDefault(h => h.Name == tag);

                if (hashtag != null)
                {
                    if (hashtag.TasksWithHashtag == null)
                        hashtag.TasksWithHashtag = new List<TaskToDo>();

                    if(!hashtag.TasksWithHashtag.Contains(task))
                        hashtag.TasksWithHashtag.Add(task);
                }
                else
                {
                    hashtag = new Hashtag()
                    {
                        Id = Guid.NewGuid(),
                        Name = tag,
                        TasksWithHashtag = new List<TaskToDo>() { task }
                    };
                    db.Hashtags.Add(hashtag);
                }
            }
            db.SaveChanges();
        }

        

        // GET: Tasks/Edit/5
        [Authorize]
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskToDo taskToDo = db.TaskToDoes.Find(id);
            if (taskToDo == null)
            {
                return HttpNotFound();
            }
            return View(taskToDo);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TaskToDo taskToDo)
        {
            if (ModelState.IsValid)
            {
                var editedTask = db.TaskToDoes.Find(taskToDo.Id);
                editedTask.Description = taskToDo.Description;
                editedTask.IsDone = taskToDo.IsDone;
                editedTask.IsFavorite = taskToDo.IsFavorite;
                db.SaveChanges();
                return RedirectToAction("IndexWithFolders");
            }
            return View(taskToDo);
        }
                

        [HttpPost]
        public ActionResult DeleteTask(Guid? id)
        {
            TaskToDo taskToDo = db.TaskToDoes.Find(id);
            if (taskToDo == null)
                return HttpNotFound();
            try
            {
                db.TaskToDoes.Remove(taskToDo);
                db.SaveChanges();
            }
            catch(System.Exception e)
            {
                WriteLog(e.Message);
            }
            
            return Json(new { success = "Valid" }, JsonRequestBehavior.AllowGet);
        }

        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
