using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ToDoList.Models
{
    public class Folder
    {
        public Guid Id { get; set; }

        [Remote("IsFolderNameAvailable", "Validation", ErrorMessage = "Folder name already exists")]
        public string Name { get; set; }
    }
}