using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ToDoList.DbInfrasrtucture;
using ToDoList.Models;

namespace ToDoList.Classes
{
    public class AutofacConfig
    {
        public static void ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);            
            builder.RegisterType<TasksDbContext>().As<ITasksDbContext>().InstancePerRequest();
            var container = builder.Build();
            System.Web.Mvc.DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}