using DataAccess.Model.BaseModel;
using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models.EIOModel
{
    public class EIOPhysicianNote : VBaseModel
    {
        public DateTime? NoteTime { get; set; }
        public string Examination { get; set; }
        public string Treatment { get; set; }
        public Guid? VisitId { get; set; }
        public string VisitTypeGroupCode { get; set; }

        [NotMapped]
        public string NoteTimeString => NoteTime.HasValue ? NoteTime.Value.ToString("HH:mm dd/MM/yyyy") : string.Empty;
        public Guid? FormId { get; set; }
    }
}