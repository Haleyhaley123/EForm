using DataAccess.Model.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models.EIOModel
{
    public class HighlyRestrictedAntimicrobialConsultMicrobiologicalResults : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Date { get; set; }
        public string Culture { get; set; }
        public string Specimen { get; set; }
        public string Others { get; set; }

        public Guid? HighlyRestrictedAntimicrobialConsultId { get; set; }
        [ForeignKey("HighlyRestrictedAntimicrobialConsultId")]
        public virtual HighlyRestrictedAntimicrobialConsult HighlyRestrictedAntimicrobialConsult { get; set; }

        public bool IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
