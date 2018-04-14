namespace ToDoList.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TaskToDoes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Description = c.String(),
                        IsDone = c.Boolean(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        IsFavorite = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        Folder_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Folders", t => t.Folder_Id)
                .Index(t => t.Folder_Id);
            
            CreateTable(
                "dbo.Folders",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaskToDoes", "Folder_Id", "dbo.Folders");
            DropIndex("dbo.TaskToDoes", new[] { "Folder_Id" });
            DropTable("dbo.Folders");
            DropTable("dbo.TaskToDoes");
        }
    }
}
