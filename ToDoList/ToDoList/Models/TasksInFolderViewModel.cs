using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToDoList.Models
{
    public class TasksInFolderViewModel
    {
        public List<TaskToDo> TasksInFolder { get; set; }

        public Folder Folder { get; set; }
    }
}