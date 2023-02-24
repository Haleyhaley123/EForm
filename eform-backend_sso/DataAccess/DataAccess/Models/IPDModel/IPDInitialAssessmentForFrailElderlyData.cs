﻿using DataAccess.Model.BaseModel;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.IPDModel
{
    public class IPDInitialAssessmentForFrailElderlyData : IEntity
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public Guid? IPDInitialAssessmentForFrailElderlyId { get; set; }
        [ForeignKey("IPDInitialAssessmentForFrailElderlyId")]
        public virtual IPDInitialAssessmentForFrailElderly IPDInitialAssessmentForFrailElderly { get; set; }
    }
}
