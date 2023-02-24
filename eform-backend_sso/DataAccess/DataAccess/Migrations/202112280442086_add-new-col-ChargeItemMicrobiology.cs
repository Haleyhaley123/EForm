﻿namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addnewcolChargeItemMicrobiology : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChargeItemMicrobiologies", "IsNotUse", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChargeItemMicrobiologies", "IsNotUse");
        }
    }
}
