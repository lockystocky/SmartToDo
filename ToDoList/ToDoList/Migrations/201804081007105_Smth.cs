namespace ToDoList.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Smth : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Hashtags",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.TaskToDoes", "Hashtag_Id", c => c.Guid());
            CreateIndex("dbo.TaskToDoes", "Hashtag_Id");
            AddForeignKey("dbo.TaskToDoes", "Hashtag_Id", "dbo.Hashtags", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaskToDoes", "Hashtag_Id", "dbo.Hashtags");
            DropIndex("dbo.TaskToDoes", new[] { "Hashtag_Id" });
            DropColumn("dbo.TaskToDoes", "Hashtag_Id");
            DropTable("dbo.Hashtags");
        }
    }
}
