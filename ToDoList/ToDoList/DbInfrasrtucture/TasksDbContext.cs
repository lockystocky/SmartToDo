using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using ToDoList.DbInfrasrtucture;

namespace ToDoList.Models
{
    public class TasksDbContext : DbContext, ITasksDbContext
    {      
    
        public TasksDbContext() : base("name=TasksDbContext")
        {
        }

        public System.Data.Entity.DbSet<ToDoList.Models.TaskToDo> TaskToDoes { get; set; }

        public System.Data.Entity.DbSet<ToDoList.Models.Hashtag> Hashtags { get; set; }

        public System.Data.Entity.DbSet<ToDoList.Models.Folder> Folders { get; set; }
    }
}
