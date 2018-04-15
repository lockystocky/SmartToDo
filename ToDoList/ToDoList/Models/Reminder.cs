using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Postal;

namespace ToDoList.Models
{
    public class Reminder
    {
        public string UserEmail { get; set; }

        public List<TaskToDo> UndoneTasks { get; set; }
    }   
}