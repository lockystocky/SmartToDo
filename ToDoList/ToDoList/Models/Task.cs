using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToDoList.Models
{
    public class TaskToDo
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public bool IsDone { get; set; }

        public DateTime StartDate { get; set; }

        public virtual Folder Folder { get; set; }

        public bool IsFavorite { get; set; }

        public Priority Priority { get; set; }

        public virtual List<Hashtag> Hashtags { get; set; }

        public string PathToAttachedFile { get; set; }
    }

    public enum Priority
    {
        High,
        Normal,
        Low
    }
}