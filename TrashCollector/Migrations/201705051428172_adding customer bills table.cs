namespace TrashCollector.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addingcustomerbillstable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CustomerBills", "UserId", "dbo.AspNetUsers");
            CreateTable(
                "dbo.CustomerCharges",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Charge = c.Double(nullable: false),
                        ApplicationUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId)
                .Index(t => t.ApplicationUserId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CustomerCharges", "ApplicationUserId", "dbo.AspNetUsers");
            DropIndex("dbo.CustomerCharges", new[] { "ApplicationUserId" });
            DropTable("dbo.CustomerCharges");
        }
    }
}
