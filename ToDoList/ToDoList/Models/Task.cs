using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace ToDoList.Models
{
    public class TaskToDo
    {
        public Guid Id { get; set; }

        public string AppUserId { get; set; }

        public string Description { get; set; }

        [DisplayName("Is done")]
        public bool IsDone { get; set; }

        [DisplayName("Start date")]
        public DateTime StartDate { get; set; }

        public virtual Folder Folder { get; set; }

        [DisplayName("Is favorite")]
        public bool IsFavorite { get; set; }

        public virtual List<Hashtag> Hashtags { get; set; }

        public string PathToAttachedFile { get; set; }
    }

    
}