namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addtabEsign : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Esigns",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        VisitId = c.Guid(nullable: false),
                        FormId = c.Guid(nullable: false),
                        FormCode = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedBy = c.String(),
                        DeletedAt = c.DateTime(),
                        CreatedBy = c.String(),
                        CreatedAt = c.DateTime(),
                        UpdatedBy = c.String(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.EIOFormConfirms", "EsignId", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EIOFormConfirms", "EsignId");
            DropTable("dbo.Esigns");
        }
    }
}
