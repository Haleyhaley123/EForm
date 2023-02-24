using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models.EsignModel
{
    public class Esigns : VBaseModel
    {
        public Guid VisitId { get; set; }
        public Guid FormId { get; set; }
        public string FormCode { get; set; }

    }
}
