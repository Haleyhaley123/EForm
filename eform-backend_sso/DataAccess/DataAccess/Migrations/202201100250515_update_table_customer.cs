namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_table_customer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "RelationshipID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "RelationshipID");
        }
    }
}
