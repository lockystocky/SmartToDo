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

        // GET: Tasks
        public ActionResult Index()
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

        public ActionResult IndexWithFolders()
        {
            TasksAndFoldersViewModel tasksAndFolders = new TasksAndFoldersViewModel();
            var tasks = db.TaskToDoes.Where(task => task.Folder.Name == "Default").ToList();           
            InsertHashtagReferences(tasks);
            tasksAndFolders.Tasks = tasks;
            var folders = db.Folders.Where(folder => folder.Name != "Default").ToList();
            tasksAndFolders.Folders = folders;

            return View(tasksAndFolders);
        }

        [Route("tasks/hashtag/{tag}")]
        public ActionResult Hashtag(string tag)
        {
            var hashtag = db.Hashtags
                .Where(h => h.Name == tag)
                .FirstOrDefault();

            if (hashtag == null)
                return HttpNotFound();

            var tasksWithHashtag = hashtag.TasksWithHashtag;

            InsertHashtagReferences(tasksWithHashtag);            

            return View(tasksWithHashtag);
        }

        // GET: Tasks/Details/5
        public ActionResult Details(Guid? id)
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

        public ActionResult GetCalendar()
        {
            Application msOutlook = new Application();
            NameSpace session = msOutlook.Session;
            Stores stores = session.Stores;
            string str = "No info ";
            foreach (Store store in stores)
            {
                MAPIFolder folder = store.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);

                str += folder.Name;
            }
            return View(str);
        }

        // GET: Tasks/Create
        public ActionResult Create()
        {
            TaskViewModel taskViewModel = new TaskViewModel();
            var folders = db.Folders.ToList();
            taskViewModel.AvailableFolders = new SelectList(folders, "Id", "Name");
            return View(taskViewModel);
        }

        public ActionResult CreateWithFile()
        {
            TaskViewModel taskViewModel = new TaskViewModel();
            var folders = db.Folders.ToList();
            taskViewModel.AvailableFolders = new SelectList(folders, "Id", "Name");
            return View(taskViewModel);
        }

        [HttpPost]
        public ActionResult CreateWithFile(TaskViewModel task, HttpPostedFileBase file)
        {

            if (ModelState.IsValid)
            {
                WriteLog("CreateWithFile, model is valid");

                if (file == null)
                    WriteLog("file is null");

                string path = "";

                if (file != null && file.ContentLength > 0)
                {
                    path = Path.Combine(Server.MapPath("~/UploadedFiles"), Path.GetFileName(file.FileName));
                    WriteLog("filename = " + Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                }
                    
                TaskToDo taskToDo = task.TaskToDo;
                taskToDo.Id = Guid.NewGuid();
                Models.Folder folder = db.Folders.Find(task.FolderId);
                if (folder == null)
                    return View(task);
                taskToDo.Folder = folder;
                taskToDo.PathToAttachedFile = path;
                db.TaskToDoes.Add(taskToDo);
                if (taskToDo.Description.Contains("#"))
                    ManageHashtags(taskToDo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            WriteLog("CreateWithFile, model is ivalid");

            return View(task);
        }

        public ActionResult CreateFolder()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateFolder(Models.Folder folder)
        {
            if (ModelState.IsValid)
            {
                folder.Id = Guid.NewGuid();
                db.Folders.Add(folder);
                db.SaveChanges();

                return RedirectToAction("Index");
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
                    if (hashtag.TasksWithHashtag == null) hashtag.TasksWithHashtag = new List<TaskToDo>();
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

        // POST: Tasks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TaskViewModel task)
        {
            
            if (ModelState.IsValid)
            { 
                TaskToDo taskToDo = task.TaskToDo;
                taskToDo.Id = Guid.NewGuid();
                taskToDo.StartDate = DateTime.Now;
                Models.Folder folder = db.Folders.Find(task.FolderId);
                if (folder == null)
                    return View(task);
                taskToDo.Folder = folder;
                db.TaskToDoes.Add(taskToDo);
                if (taskToDo.Description.Contains("#"))
                    ManageHashtags(taskToDo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(task);
        }

        // GET: Tasks/Edit/5
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
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,IsDone,StartDate,IsFavorite")] TaskToDo taskToDo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(taskToDo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(taskToDo);
        }

        // GET: Tasks/Delete/5
        public ActionResult Delete(Guid? id)
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

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            TaskToDo taskToDo = db.TaskToDoes.Find(id);
            db.TaskToDoes.Remove(taskToDo);
            db.SaveChanges();
            return RedirectToAction("Index");
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
