namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class insert_IsHasFallRiskScreening_EOCs : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EOCs", "IsHasFallRiskScreening", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EOCs", "IsHasFallRiskScreening");
        }
    }
}
