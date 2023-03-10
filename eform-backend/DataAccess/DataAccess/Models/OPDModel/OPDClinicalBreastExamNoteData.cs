using DataAccess.Model.BaseModel;
using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models.OPDModel
{
    public class OPDClinicalBreastExamNoteData : VBaseModel
    {
    
        public string Code { get; set; }
        public string Value { get; set; }
        public string EnValue { get; set; }
        public Nullable<Guid> OPDClinicalBreastExamNoteId { get; set; }
        [ForeignKey("OPDClinicalBreastExamNoteId")]
        public virtual OPDClinicalBreastExamNote OPDClinicalBreastExamNote { get; set; }
    }
}
