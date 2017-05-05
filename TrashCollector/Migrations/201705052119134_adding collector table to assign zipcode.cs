namespace TrashCollector.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addingcollectortabletoassignzipcode : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Collectors",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Collectors", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Collectors", new[] { "UserId" });
            DropTable("dbo.Collectors");
        }
    }
}
