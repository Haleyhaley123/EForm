namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_table_hanoverchecklist_add_column : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EDHandOverCheckLists", "NurseAcceptTime", c => c.DateTime());
            AddColumn("dbo.EDHandOverCheckLists", "PhysicianAcceptTime", c => c.DateTime());
            AddColumn("dbo.EOCHandOverCheckLists", "NurseAcceptTime", c => c.DateTime());
            AddColumn("dbo.EOCHandOverCheckLists", "PhysicianAcceptTime", c => c.DateTime());
            AddColumn("dbo.IPDHandOverCheckLists", "NurseAcceptTime", c => c.DateTime());
            AddColumn("dbo.IPDHandOverCheckLists", "PhysicianAcceptTime", c => c.DateTime());
            AddColumn("dbo.OPDHandOverCheckLists", "NurseAcceptTime", c => c.DateTime());
            AddColumn("dbo.OPDHandOverCheckLists", "PhysicianAcceptTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OPDHandOverCheckLists", "PhysicianAcceptTime");
            DropColumn("dbo.OPDHandOverCheckLists", "NurseAcceptTime");
            DropColumn("dbo.IPDHandOverCheckLists", "PhysicianAcceptTime");
            DropColumn("dbo.IPDHandOverCheckLists", "NurseAcceptTime");
            DropColumn("dbo.EOCHandOverCheckLists", "PhysicianAcceptTime");
            DropColumn("dbo.EOCHandOverCheckLists", "NurseAcceptTime");
            DropColumn("dbo.EDHandOverCheckLists", "PhysicianAcceptTime");
            DropColumn("dbo.EDHandOverCheckLists", "NurseAcceptTime");
        }
    }
}
