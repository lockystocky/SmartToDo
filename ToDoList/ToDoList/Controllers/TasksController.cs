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
using ToDoList.DbInfrasrtucture;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class TasksController : Controller
    {
        private ITasksDbContext db;

        public TasksController(ITasksDbContext context)
        {
            this.db = context;
        }

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

        private void InsertHashtagReferences(List<TaskToDo> tasks)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                Regex regex = new Regex(@"#\w+");
                foreach (Match match in regex.Matches(tasks[i].Description))
                {
                    string tag = match.Value;
                    string href = "/tasks/hashtag/" + tag.Substring(1);
                    tasks[i].Description = tasks[i].Description
                        .Replace(tag, String.Format("<a href=\"{0}\">{1}</a>", href, tag));
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

            var tasksInFolder = db.TaskToDoes
                .Where(task => task.Folder.Name == folderName)
                .ToList();
            
            InsertHashtagReferences(tasksInFolder);
            return View(tasksInFolder);
        }

        [Authorize]
        public ActionResult IndexWithFolders()
        {
            var currentUserId = GetCurrentUserId();

            TasksAndFoldersViewModel tasksAndFolders = new TasksAndFoldersViewModel();

            var tasksInDefaultFolder = db.TaskToDoes
                .Where(task => task.Folder.Name == "Default"
                        && task.AppUserId == currentUserId)
                .ToList();

            InsertHashtagReferences(tasksInDefaultFolder);

            tasksAndFolders.Tasks = tasksInDefaultFolder;

            var folders = db.Folders
                .Where(folder => folder.Name != "Default" 
                        && folder.AppUserId == currentUserId)
                .ToList();

            tasksAndFolders.Folders = folders;

            return View(tasksAndFolders);
        }
        
        [Authorize]
        [Route("tasks/hashtag/{tag}")]
        public ActionResult Hashtag(string tag)
        {
            var currentUserId = GetCurrentUserId();

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

        private string GetCurrentUserId()
        {
            var context = new ApplicationDbContext();
            return User.Identity.GetUserId();
        }


        private Models.Folder CreateDefaultFolder(string userId)
        {
            Models.Folder defaultFolder = new Models.Folder
            {
                Name = "Default",
                Id = Guid.NewGuid(),
                AppUserId = userId
            };
            db.Folders.Add(defaultFolder);
            db.SaveChanges();

            return defaultFolder;
        }

        [Authorize]
        //creates task with attachment
        public ActionResult CreateWithFile()
        {
            var currentUserId = GetCurrentUserId();

            TaskViewModel taskViewModel = new TaskViewModel();
            var folders = db.Folders.Where(f => f.AppUserId == currentUserId).ToList();
            bool defaultFolderExists = folders.Where(f => f.Name == "Default").Count() > 0;
            if (!defaultFolderExists)
            {
                var newDefaultFolder = CreateDefaultFolder(currentUserId);
                folders.Add(newDefaultFolder);
            }
            taskViewModel.AvailableFolders = new SelectList(folders, "Id", "Name");
            return View(taskViewModel);
        }

        private string SaveUploadedFile(HttpPostedFileBase file)
        {
            string pathToFile = "";
            if (file != null && file.ContentLength > 0)
            {
                pathToFile = Path.Combine(Server.MapPath("~/UploadedFiles"), Path.GetFileName(file.FileName));
                file.SaveAs(pathToFile);
            }
            return pathToFile;
        }

        [HttpPost]
        [Authorize]
        //creates task with attachment
        public ActionResult CreateWithFile(TaskViewModel task, HttpPostedFileBase file)
        {

            if (ModelState.IsValid)
            {
                var currentUserId = GetCurrentUserId();

                string pathToFile = SaveUploadedFile(file);                 
                    
                TaskToDo taskToDo = task.TaskToDo;
                taskToDo.Id = Guid.NewGuid();
                Models.Folder folder = db.Folders.Find(task.FolderId);             

                taskToDo.Folder = folder;
                taskToDo.StartDate = DateTime.Now;
                taskToDo.AppUserId = currentUserId;
                taskToDo.PathToAttachedFile = pathToFile;

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
                folder.AppUserId = GetCurrentUserId();
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

               Hashtag hashtag = db.Hashtags.FirstOrDefault(h => h.Name == tag);

                if (hashtag == null)
                    hashtag = CreateHashtag(tag, task);
                
                if (hashtag.TasksWithHashtag == null)
                    hashtag.TasksWithHashtag = new List<TaskToDo>();

                if(!hashtag.TasksWithHashtag.Contains(task))
                    hashtag.TasksWithHashtag.Add(task);
               
            }
            db.SaveChanges();
        }

        private Hashtag CreateHashtag(string tag, TaskToDo task)
        {
            var hashtag = new Hashtag()
            {
                Id = Guid.NewGuid(),
                Name = tag,
                TasksWithHashtag = new List<TaskToDo>() { task }
            };
            db.Hashtags.Add(hashtag);
            return hashtag;
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
            /*if (disposing)
            {
                db.Dispose();
            }*/
            base.Dispose(disposing);
        }
    }
}
