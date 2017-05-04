namespace TrashCollector.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Customerdatestableupdatenullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CustomerDates", "AlternatePickup", c => c.DateTime());
            AlterColumn("dbo.CustomerDates", "VacationStart", c => c.DateTime());
            AlterColumn("dbo.CustomerDates", "VacationEnd", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CustomerDates", "VacationEnd", c => c.DateTime(nullable: false));
            AlterColumn("dbo.CustomerDates", "VacationStart", c => c.DateTime(nullable: false));
            AlterColumn("dbo.CustomerDates", "AlternatePickup", c => c.DateTime(nullable: false));
        }
    }
}
