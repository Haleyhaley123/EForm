namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeColumnsTableIpdVitalSignForAdult_V1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "IPD_ID");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "PID");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "BREATH_RATE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "SPO2");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "LOW_BP");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "HIGH_BP");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "PULSE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "TEMPERATURE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "SENSE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "RESPIRATORY_SUPPORT");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "PAIN_SCORE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "CAPILLARY_BLOOD");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "VIP_SCORE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_T");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_T_VALUE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_P");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_P_VALUE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_M");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_M_VALUE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_S");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_S_VALUE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_AN");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_AN_VALUE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_D");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_D_VALUE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_N");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_N_VALUE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_PH");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_PH_VALUE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_NT");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_NT_VALUE");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_DL");
            DropColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_DL_VALUE");
        }
        
        public override void Down()
        {
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_DL_VALUE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_DL", c => c.Boolean(nullable: false));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_NT_VALUE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_NT", c => c.Boolean(nullable: false));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_PH_VALUE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_PH", c => c.Boolean(nullable: false));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_N_VALUE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_OUT_N", c => c.Boolean(nullable: false));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_D_VALUE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_D", c => c.Boolean(nullable: false));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_AN_VALUE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_AN", c => c.Boolean(nullable: false));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_S_VALUE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_S", c => c.Boolean(nullable: false));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_M_VALUE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_M", c => c.Boolean(nullable: false));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_P_VALUE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_P", c => c.Boolean(nullable: false));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_T_VALUE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "FLUID_IN_T", c => c.Boolean(nullable: false));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "VIP_SCORE", c => c.Int());
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "CAPILLARY_BLOOD", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "PAIN_SCORE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "RESPIRATORY_SUPPORT", c => c.String());
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "SENSE", c => c.String());
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "TEMPERATURE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "PULSE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "HIGH_BP", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "LOW_BP", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "SPO2", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "BREATH_RATE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "PID", c => c.String(maxLength: 20));
            AddColumn("dbo.IPD_VITALSIGN_ADULT", "IPD_ID", c => c.Guid());
        }
    }
}
