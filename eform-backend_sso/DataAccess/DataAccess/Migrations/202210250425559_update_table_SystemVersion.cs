namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_table_SystemVersion : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.SystemVersions", newName: "AppVersions");
            AddColumn("dbo.AppVersions", "Order", c => c.Int(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AppVersions", "Order");
            RenameTable(name: "dbo.AppVersions", newName: "SystemVersions");
        }
    }
}
