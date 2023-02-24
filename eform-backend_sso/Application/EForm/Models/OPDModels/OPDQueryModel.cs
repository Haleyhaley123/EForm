using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EForm.Models.OPDModels
{
    public class listMedicalRecordIsDeploy
    {
        public string FormCode { get; set; }
        public string ViName { get; set; }
        public string EnName { get; set; }
        public string Type { get; set; }

    }
    public class OPDQueryModel
    {
        public Guid Id { get; set; }
        public string VisitCode { get; set; }
        public bool IsTelehealth { get; set; }
        public bool IsRetailService { get; set; }
        public bool IsBooked { get; set; }
        public DateTime AdmittedDate { get; set; }
        public string AdmittedDateDate { get; set; }
        public DateTime? BookingTime { get; set; }
        public string CustomerPID { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerFullname { get; set; }
        public DateTime? CustomerDateOfBirth { get; set; }
        public bool? CustomerIsAllergy { get; set; }
        public string CustomerAllergy { get; set; }
        public string CustomerKindOfAllergy { get; set; }
        public Guid EDStatusId { get; set; }
        public DateTime? EDStatusCreatedAt { get; set; }
        public string EDStatusEnName { get; set; }
        public string EDStatusViName { get; set; }
        public string EDStatusCode { get; set; }
        public Guid? PrimaryDoctorId { get; set; }
        public string PrimaryDoctorUsername { get; set; }
        public Guid? AuthorizedDoctorId { get; set; }
        public string AuthorizedDoctorUsername { get; set; }
        public Guid? PrimaryNurseId { get; set; }
        public string PrimaryNurseUsername { get; set; }
        public bool? IsHasFallRiskScreening { get; set; }
        public Guid? OPDFallRiskScreeningId { get; set; }
        public Guid? OPDInitialAssessmentForShortTermId { get; set; }
        public Guid? OPDInitialAssessmentForTelehealthId { get; set; }

        public string CustomerPIDOH { get; set; }
        public string UnlockFor { get; set; }
        public bool IsVip { get; set; }
        public Guid SpecialtyId { get; set; }
        public bool? IsAllergy { get; set; }
        public string Allergy { get; set; }
        public string KindOfAllergy { get; set; }
        public bool IsAnesthesia { get; set; }
        public Guid? ClinicId { get; set; }
        public string ClinicViName { get; set; }
        public bool? IsConsultation { get; set; }
    }
}