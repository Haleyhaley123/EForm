using DataAccess.Models.OPDModel;
using DataAccess.Repository;
using EForm.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EForm.Utils
{
    public class VisitAllergy
    {
        private dynamic Visit;
        private string VisitTypeGroup;
        private IUnitOfWork unitOfWork = new EfUnitOfWork();

        public VisitAllergy(dynamic visit, string visit_type)
        {
            this.Visit = visit;
            this.VisitTypeGroup = visit_type;
        }

        public string GetAllergy()
        {
            if (this.VisitTypeGroup == "ED")
                return "";
            else if (this.VisitTypeGroup == "OPD")
                return GetOPDAllergy();
            return "";
        }
        public void UpdateAllergy(Dictionary<string, string> all_dct)
        {
            if(all_dct.Count > 0)
            {
                if (all_dct["YES"].Trim().ToLower() == "true")
                {
                    Visit.IsAllergy = true;
                    Visit.KindOfAllergy = all_dct["KOA"];
                    Visit.Allergy = all_dct["ANS"];
                }
                else if (all_dct["NOO"].Trim().ToLower() == "true")
                {
                    Visit.IsAllergy = false;
                    Visit.KindOfAllergy = "";
                    Visit.Allergy = "Không";
                }
                else if (all_dct["NPA"].Trim().ToLower() == "true")
                {
                    Visit.IsAllergy = null;
                    Visit.KindOfAllergy = "";
                    Visit.Allergy = "Không xác định";
                }
                else
                {
                    Visit.IsAllergy = null;
                    Visit.KindOfAllergy = "";
                    Visit.Allergy = "";
                }
            }     
        }
        private string GetOPDAllergy()
        {
            if (this.Visit.IsTelehealth)
                return GetAllergyInInitialAssessmentForTelehealth();
            return GetAllergyInInitialAssessmentForShortTerm();
        }
        private string GetAllergyInInitialAssessmentForTelehealth()
        {
            Guid form_id = this.Visit.OPDInitialAssessmentForTelehealthId;

            var all = unitOfWork.OPDInitialAssessmentForTelehealthDataRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                e.OPDInitialAssessmentForTelehealthId == form_id &&
                !string.IsNullOrEmpty(e.Code) &&
                e.Code.Contains("OPDIAFTPALLANS")
            )?.Value;
            if (!string.IsNullOrEmpty(all))
                return all;

            var koa = unitOfWork.OPDInitialAssessmentForTelehealthDataRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                e.OPDInitialAssessmentForTelehealthId == form_id &&
                !string.IsNullOrEmpty(e.Code) &&
                e.Code.Contains("OPDIAFTPALLKOA")
            )?.Value;
            if (!string.IsNullOrEmpty(koa))
            {
                var koa_value = "";
                foreach (var i in koa.Split(','))
                    koa_value += $"{Constant.KIND_OF_ALLERGIC[i]}, ";
                return koa_value.Substring(0, koa_value.Length - 2);
            }

            var na = unitOfWork.OPDInitialAssessmentForTelehealthDataRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                e.OPDInitialAssessmentForTelehealthId == form_id &&
                !string.IsNullOrEmpty(e.Code) &&
                e.Code.Contains("OPDIAFTPALLNPA")
            )?.Value;
            if (!string.IsNullOrEmpty(na) && na.ToLower() == "true")
                return "Không xác định";

            return "Không";
        }
        private string GetAllergyInInitialAssessmentForShortTerm()
        {
            Guid form_id = this.Visit.OPDInitialAssessmentForShortTermId;
            var all = unitOfWork.OPDInitialAssessmentForShortTermDataRepository.FirstOrDefault(
                    e => !e.IsDeleted &&
                    e.OPDInitialAssessmentForShortTermId == form_id &&
                    !string.IsNullOrEmpty(e.Code) &&
                    e.Code.Contains("OPDIAFSTOPALLANS")
                )?.Value;
            if (!string.IsNullOrEmpty(all))
                return all;

            var koa = unitOfWork.OPDInitialAssessmentForShortTermDataRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                e.OPDInitialAssessmentForShortTermId == form_id &&
                !string.IsNullOrEmpty(e.Code) &&
                e.Code.Contains("OPDIAFSTOPALLKOA")
            )?.Value;
            if (!string.IsNullOrEmpty(koa))
            {
                var koa_value = "";
                foreach (var i in koa.Split(','))
                    koa_value += $"{Constant.KIND_OF_ALLERGIC[i]}, ";
                return koa_value.Substring(0, koa_value.Length - 2);
            }

            var na = unitOfWork.OPDInitialAssessmentForShortTermDataRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                e.OPDInitialAssessmentForShortTermId == form_id &&
                !string.IsNullOrEmpty(e.Code) &&
                e.Code.Contains("OPDIAFSTOPALLNPA")
            )?.Value;
            if (!string.IsNullOrEmpty(na) && na.ToLower() == "true")
                return "Không xác định";

            return "Không";
        }
    }
}