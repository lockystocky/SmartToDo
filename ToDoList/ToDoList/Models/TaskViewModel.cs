using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ToDoList.Models
{
    public class TaskViewModel
    {
        public TaskToDo TaskToDo { get; set; }

        [DisplayName("Folder")]
        public Guid FolderId { get; set; }

        //[DisplayName("Folder")]
        public SelectList AvailableFolders { get; set; }
        
    }
}