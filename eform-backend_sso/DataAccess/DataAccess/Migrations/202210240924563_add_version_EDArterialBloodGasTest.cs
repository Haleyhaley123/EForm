namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_version_EDArterialBloodGasTest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EDArterialBloodGasTests", "Version", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EDArterialBloodGasTests", "Version");
        }
    }
}
