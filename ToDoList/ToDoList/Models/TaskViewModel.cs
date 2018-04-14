using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ToDoList.Models
{
    public class TaskViewModel
    {
        public TaskToDo TaskToDo { get; set; }

        // public IEnumerable<SelectListItem> AvailableFolders { get; set; }
        public Guid FolderId { get; set; }


        public SelectList AvailableFolders { get; set; }

        // public TaskViewModel() { }

    }
}