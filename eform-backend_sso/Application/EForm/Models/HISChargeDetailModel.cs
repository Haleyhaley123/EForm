using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EForm.Models
{
    public class HISChargeDetailModel
    {
        public Guid? ChargeDetailId { get; set; }
        public Guid? NewChargeId { get; set; }
        public string FillerOrderNumber { get; set; }
        public string PlacerOrderNumber { get; set; }
        public string PaymentStatus { get; set; }
        public string RadiologyScheduledStatus { get; set; }
        public string PlacerOrderStatus { get; set; }
        public string SpecimenStatus { get; set; }
        public DateTime? ChargeDeletedDate { get; set; }
    }
}
