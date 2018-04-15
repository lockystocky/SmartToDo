using Hangfire;
using Microsoft.AspNet.Identity;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace ToDoList.Models
{
    public class Hangfire
    {
        public static void ConfigureHangfire(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
                .UseSqlServerStorage("DefaultConnection");

            app.UseHangfireDashboard("/jobs");
            app.UseHangfireServer();
        }

        public static void InitializeJobs()
        {
            RecurringJob.AddOrUpdate(() => SendReminders(), Cron.Hourly);
        }

        public static void SendReminders()
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("locky.stocky24@gmail.com", "lheufhsrf24"),
                EnableSsl = true
            };

            var reminders = GetUndoneTasks();

            foreach (var reminder in reminders)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine("Dear user,");
                message.AppendLine();
                message.AppendLine("Hi! Let's complete undone tasks. You were planning to do:");
                message.AppendLine();

                foreach (var task in reminder.UndoneTasks)
                {
                    message.AppendLine(task.Description);
                    message.AppendLine();
                }

                message.AppendLine();
                message.AppendLine("Lots of love,");
                message.AppendLine("Your To Do Remainder");
                client.Send("locky.stocky24@gmail.com", reminder.UserEmail, "To Do Remainder", message.ToString());
            }


        }

        public static List<Reminder> GetUndoneTasks()
        {
             TasksDbContext db = new TasksDbContext();
             var tasksGroups = db.TaskToDoes
                .Where(task => !task.IsDone && task.AppUserId != null && task.AppUserId.Length > 0)
                .GroupBy(t => t.AppUserId);

            List<Reminder> reminders = new List<Reminder>();
           
            foreach (var group in tasksGroups)
            {
                Reminder reminderEmail = new Reminder();
                reminderEmail.UserEmail = GetUserEmail(group.Key);
                reminderEmail.UndoneTasks = new List<TaskToDo>();

                foreach (var task in group)
                {
                    reminderEmail.UndoneTasks.Add(task);
                }

                if (reminderEmail.UndoneTasks.Count > 0)
                    reminders.Add(reminderEmail);
            }
            return reminders;
        }

        public static string GetUserEmail(string userId)
        {
            var appContext = new ApplicationDbContext();
            var user = appContext.Users.FirstOrDefault(u => u.Id == userId);
            return user.Email;
        }
    }
}