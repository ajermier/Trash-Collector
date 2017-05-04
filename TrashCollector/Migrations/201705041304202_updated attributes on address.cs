namespace TrashCollector.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedattributesonaddress : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CustomerAddresses", "Address1", c => c.String(nullable: false));
            AlterColumn("dbo.CustomerAddresses", "City", c => c.String(nullable: false));
            AlterColumn("dbo.CustomerAddresses", "State", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CustomerAddresses", "State", c => c.String());
            AlterColumn("dbo.CustomerAddresses", "City", c => c.String());
            AlterColumn("dbo.CustomerAddresses", "Address1", c => c.String());
        }
    }
}
