using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToDoList.Models
{
    public class TasksAndFoldersViewModel
    {
        public IEnumerable<TaskToDo> Tasks { get; set; }

        public IEnumerable<Folder> Folders { get; set; }
    }
}