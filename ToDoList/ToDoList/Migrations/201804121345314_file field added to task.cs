namespace ToDoList.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class filefieldaddedtotask : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TaskToDoes", "PathToAttachedFile", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TaskToDoes", "PathToAttachedFile");
        }
    }
}
