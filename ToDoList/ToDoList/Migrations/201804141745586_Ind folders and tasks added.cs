namespace ToDoList.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Indfoldersandtasksadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Folders", "AppUserId", c => c.String());
            AddColumn("dbo.TaskToDoes", "AppUserId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TaskToDoes", "AppUserId");
            DropColumn("dbo.Folders", "AppUserId");
        }
    }
}
