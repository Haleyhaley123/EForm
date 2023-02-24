namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcolformidtoUploadImagetable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UploadImages", "FormId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UploadImages", "FormId");
        }
    }
}
