namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_table_SystemVersion : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SystemVersions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Lable = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        Version = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedBy = c.String(),
                        DeletedAt = c.DateTime(),
                        CreatedBy = c.String(),
                        CreatedAt = c.DateTime(),
                        UpdatedBy = c.String(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.EDs", "Version", c => c.Int(nullable: false, defaultValue:1));
            AddColumn("dbo.EOCs", "Version", c => c.Int(nullable: false, defaultValue:1));
            AddColumn("dbo.IPDs", "Version", c => c.Int(nullable: false, defaultValue:1));
            AddColumn("dbo.OPDs", "Version", c => c.Int(nullable: false, defaultValue:1));
            DropColumn("dbo.EDArterialBloodGasTests", "Version");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EDArterialBloodGasTests", "Version", c => c.Int(nullable: false));
            DropColumn("dbo.OPDs", "Version");
            DropColumn("dbo.IPDs", "Version");
            DropColumn("dbo.EOCs", "Version");
            DropColumn("dbo.EDs", "Version");
            DropTable("dbo.SystemVersions");
        }
    }
}
