namespace ToDoList.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class manytomany : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TaskToDoes", "Hashtag_Id", "dbo.Hashtags");
            DropIndex("dbo.TaskToDoes", new[] { "Hashtag_Id" });
            CreateTable(
                "dbo.TaskToDoHashtags",
                c => new
                    {
                        TaskToDo_Id = c.Guid(nullable: false),
                        Hashtag_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.TaskToDo_Id, t.Hashtag_Id })
                .ForeignKey("dbo.TaskToDoes", t => t.TaskToDo_Id, cascadeDelete: true)
                .ForeignKey("dbo.Hashtags", t => t.Hashtag_Id, cascadeDelete: true)
                .Index(t => t.TaskToDo_Id)
                .Index(t => t.Hashtag_Id);
            
            DropColumn("dbo.TaskToDoes", "Hashtag_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TaskToDoes", "Hashtag_Id", c => c.Guid());
            DropForeignKey("dbo.TaskToDoHashtags", "Hashtag_Id", "dbo.Hashtags");
            DropForeignKey("dbo.TaskToDoHashtags", "TaskToDo_Id", "dbo.TaskToDoes");
            DropIndex("dbo.TaskToDoHashtags", new[] { "Hashtag_Id" });
            DropIndex("dbo.TaskToDoHashtags", new[] { "TaskToDo_Id" });
            DropTable("dbo.TaskToDoHashtags");
            CreateIndex("dbo.TaskToDoes", "Hashtag_Id");
            AddForeignKey("dbo.TaskToDoes", "Hashtag_Id", "dbo.Hashtags", "Id");
        }
    }
}
