namespace ToDoList.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Nameschanged : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.TaskToDoes", "Priority");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TaskToDoes", "Priority", c => c.Int(nullable: false));
        }
    }
}
