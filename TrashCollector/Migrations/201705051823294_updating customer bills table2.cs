namespace TrashCollector.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatingcustomerbillstable2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CustomerCharges", "Date", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CustomerCharges", "Date", c => c.DateTime(nullable: false));
        }
    }
}
