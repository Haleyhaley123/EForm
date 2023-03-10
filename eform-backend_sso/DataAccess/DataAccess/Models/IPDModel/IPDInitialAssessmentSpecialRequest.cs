using DataAccess.Model.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.IPDModel
{
    public class IPDInitialAssessmentSpecialRequest : IEntity
    {
        [Key]
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? IPDId { get; set; }
        [ForeignKey("IPDId")]
        public virtual IPD IPD { get; set; }
        public string Code { get; set; }
        public string Group { get; set; }
        public string ViName { get; set; }
        public string EnName { get; set; }
        public string ViValue { get; set; }
        public string EnValue { get; set; }
        public int Order { get; set; }
        public bool IsKey { get; set; }
        public string DataType { get; set; }
    }
}
