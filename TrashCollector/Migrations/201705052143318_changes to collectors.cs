namespace TrashCollector.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changestocollectors : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Collectors", "ZipCode", c => c.Int(nullable: false));
            AddColumn("dbo.Collectors", "EmployeeId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Collectors", "EmployeeId");
            DropColumn("dbo.Collectors", "ZipCode");
        }
    }
}
