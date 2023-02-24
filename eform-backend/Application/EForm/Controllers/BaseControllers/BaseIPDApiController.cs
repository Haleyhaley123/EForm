using DataAccess.Models;
using DataAccess.Models.IPDModel;
using EForm.BaseControllers;
using EForm.Common;
using EForm.Models.IPDModels;
using EForm.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EForm.Controllers.BaseControllers
{
    public class BaseIPDApiController : BaseApiController
    {
        protected TransferInfoModel GetFirstIpd(IPD ipd)
        {
            var spec = ipd.Specialty;
            var current_doctor = ipd.PrimaryDoctor;

            var transfers = new IPDTransfer(ipd).GetListInfo();
            TransferInfoModel first_ipd = null;
            if (transfers.Count() > 0)
            {
                first_ipd = transfers.FirstOrDefault(e => e.CurrentType == "ED" || e.CurrentType == "IPD" && (string.IsNullOrEmpty(e.CurrentSpecialtyCode) || !e.CurrentSpecialtyCode.Contains("PTTT")));
            }
            else
            {
                first_ipd = new TransferInfoModel()
                {
                    CurrentRawDate = ipd.AdmittedDate,
                    CurrentSpecialty = new { spec?.ViName, spec?.EnName },
                    CurrentDoctor = new { current_doctor?.Username, current_doctor?.Fullname, current_doctor?.DisplayName },
                    CurrentDate = ipd.AdmittedDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                };
            }
            if (first_ipd == null)
            {
                first_ipd = new TransferInfoModel()
                {
                    CurrentRawDate = ipd.AdmittedDate,
                    CurrentSpecialty = new { spec?.ViName, spec?.EnName },
                    CurrentDoctor = new { current_doctor?.Username, current_doctor?.Fullname, current_doctor?.DisplayName },
                    CurrentDate = ipd.AdmittedDate.ToString(Constant.TIME_DATE_FORMAT_WITHOUT_SECOND),
                };
            }
            return first_ipd;
        }
        protected void UpdateVisit(IPD visit)
        {
            unitOfWork.IPDRepository.Update(visit);
            unitOfWork.Commit();
        }
        protected List<MasterData> GetSpecialtySetupForm(Guid specialtyId, List<MasterData> master_dataSetup)
        {
            var spec_setup = unitOfWork.IPDSetupMedicalRecordRepository
                            .Find(e => !e.IsDeleted && e.SpecialityId == specialtyId && e.IsDeploy)
                            .ToList();

            var list_menu = from spec in spec_setup
                            join md in master_dataSetup
                            on spec.Formcode equals md.Form
                            where spec.FormType == "MedicalRecords"
                            select md;

            var list_tabInForm = from res in list_menu
                                 join md in master_dataSetup
                                 on res.Form equals md.Group
                                 select md;

            List<MasterData> result = list_menu.Concat(list_tabInForm).GroupBy(e => new { e.Form, e.Code }).Select(e => e.FirstOrDefault()).Where(e => e.Level > 1).ToList();
            return result;
        }
        protected List<dynamic> GetMedicalRecordsSavedOrSetup(Guid visitId, List<MasterData> md_setup, List<MasterData> specialty_setupForm)
        {
            List<MasterData> forms_md = GetFormPatienSaved(visitId, md_setup);

            List<MasterData> list_temp = forms_md.Concat(specialty_setupForm).GroupBy(e => new { e.Code, e.Form }).Select(e => e.FirstOrDefault()).Where(e => e.Level == 2).OrderBy(e => e.Order).ToList();
            List<dynamic> forms_result = new List<dynamic>();
            foreach (var item in list_temp)
            {
                if (item.DataType == "BENHAN")
                {
                    var part2 = new { FormCode = item.Form, ViName = item.ViName, EnName = item.EnName, Type = item.Code + "/Part2", Role = new int[] { 1, 3 } };

                    var part1 = new { FormCode = item.Form, ViName = item.ViName, EnName = item.EnName, Type = item.Code + "/Part1", Role = new int[] { 2 } };

                    var print = new { FormCode = item.Form, ViName = item.ViName, EnName = item.EnName, Type = item.Code + "/Print", Role = new int[] { 4 } };

                    forms_result.Add(part2);
                    forms_result.Add(part1);
                    forms_result.Add(print);
                }
                else
                {
                    var obj = new { FormCode = item.Form, ViName = item.ViName, EnName = item.EnName, Type = item.Code, Role = new int[] { } };

                    forms_result.Add(obj);
                }
            }
            return forms_result;
        }

        protected List<MasterData> GetFormPatienSaved(Guid visitId, List<MasterData> masterdata_setup)
        {
            List<IPDMedicalRecordOfPatients> forms_patient = unitOfWork.IPDMedicalRecordOfPatientRepository
                                                            .Find(e => !e.IsDeleted && e.VisitId == visitId)
                                                            .ToList();

            var md_list_form = from e in forms_patient
                               join md in masterdata_setup
                               on e.FormCode equals md.Form
                               select md;
            return md_list_form.ToList();
        }

        protected List<MasterData> GetMasterDatasSetUpForm()
        {
            List<MasterData> md = unitOfWork.MasterDataRepository
                                .Find(e => !e.IsDeleted && e.Note == "IPD" && e.DataType != null && (e.DataType.ToUpper() == "BENHAN" || e.DataType.ToUpper() == "PromissoryNote".ToUpper() || e.DataType.ToUpper() == "BIEUMAU"))
                                .ToList();
            return md;
        }
    }
}
