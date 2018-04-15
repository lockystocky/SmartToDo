using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToDoList.DbInfrasrtucture
{
    public interface ITasksDbContext
    {
        System.Data.Entity.DbSet<ToDoList.Models.TaskToDo> TaskToDoes { get; set; }

        System.Data.Entity.DbSet<ToDoList.Models.Hashtag> Hashtags { get; set; }

        System.Data.Entity.DbSet<ToDoList.Models.Folder> Folders { get; set; }

        int SaveChanges();

    }
}