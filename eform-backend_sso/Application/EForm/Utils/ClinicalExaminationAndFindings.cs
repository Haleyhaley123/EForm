using DataAccess.Models.OPDModel;
using DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EForm.Utils
{
    public class ClinicalExaminationAndFindings
    {
        private IUnitOfWork unitOfWork = new EfUnitOfWork();
        private string ClinicCode;
        private string ClinicData;
        private Guid? OENId;
        private bool IsTelehealth;
        private int Version;

        public ClinicalExaminationAndFindings(string clinic_code, string clinic_data, Guid oen_id, bool is_telehealth, int version)
        {
            this.ClinicCode = clinic_code;
            this.ClinicData = clinic_data;
            this.OENId = oen_id;
            this.IsTelehealth = is_telehealth;
            this.Version = version;
        }

        public dynamic GetData()
        {
            if (this.IsTelehealth)
                return GetOutPatientExaminationNoteDataFreeTextOnly(new string[] { "OPDOENSDBPANS" });

            if (string.IsNullOrEmpty(ClinicCode))
                return GetOutPatientExaminationNoteDataFreeTextOnly(new string[] { "OPDOENCEFANS" });

            return GetDataOENByVersion();
        }
        private dynamic GetOutPatientExaminationNoteDataFreeTextOnly(string[] code_list)
        {
            return (from data in unitOfWork.OPDOutpatientExaminationNoteDataRepository.AsQueryable()
                    .Where(
                    i => !i.IsDeleted &&
                    i.OPDOutpatientExaminationNoteId == this.OENId &&
                    !string.IsNullOrEmpty(i.Code) &&
                    code_list.Contains(i.Code)
                    )
                    join master in unitOfWork.MasterDataRepository.AsQueryable() on data.Code equals master.Code into ulist
                    from master in ulist.DefaultIfEmpty()
                    select new { master.ViName, master.EnName, data.Value, master.Code, master.Order, master.Group })
                    .OrderBy(e => e.Order)
                    .ToList();
        }
        private string GetMasterDataOutpatientExaminationNoteValue(string code)
        {
            return unitOfWork.OPDOutpatientExaminationNoteDataRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                e.OPDOutpatientExaminationNoteId != null &&
                e.OPDOutpatientExaminationNoteId == this.OENId &&
                !string.IsNullOrEmpty(e.Code) &&
                e.Code.Equals(code)
            )?.Value;
        }
        private dynamic GetOutpatientExaminationNoteDataRadioWithNull(List<dynamic> options)
        {
            foreach (var opt in options)
            {
                var opt_value = GetMasterDataOutpatientExaminationNoteValue(opt.Code);
                if (!string.IsNullOrEmpty(opt_value) && opt_value.Equals("true", StringComparison.OrdinalIgnoreCase))
                    return GetOutPatientExaminationNoteDataFreeTextOnly(opt.MasterString);
            }
            return new List<string>();
        }
        private dynamic GetOutpatientExaminationNoteDataRadioYesNo(dynamic yes_option, dynamic no_option)
        {
            if (yes_option == null || no_option == null)
                return new List<string>();

            var yes_opt_value = GetMasterDataOutpatientExaminationNoteValue(yes_option.Code);
            if (!string.IsNullOrEmpty(yes_opt_value) && yes_opt_value.Equals("true", StringComparison.OrdinalIgnoreCase))
                return GetOutPatientExaminationNoteDataFreeTextOnly(yes_option.MasterString);
            return GetOutPatientExaminationNoteDataFreeTextOnly(no_option.MasterString);
        }
        private dynamic GetOutpatientExaminationNoteDataMultiSelect(List<dynamic> options)
        {
            var results = new List<dynamic>();
            foreach (var opt in options)
            {
                var opt_value = GetMasterDataOutpatientExaminationNoteValue(opt.Code);
                if (!string.IsNullOrEmpty(opt_value) && opt_value.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    var datas = GetOutPatientExaminationNoteDataFreeTextOnly(opt.MasterString);
                    results.AddRange(datas);
                }
            }
            return results.Distinct().ToList();
        }

        private List<dynamic> SortData(List<dynamic> datas)
        {
            string[] code_option = { "OPDOENYTDNGL", "OPDOENYTDTRE", "OPDOENYTDKBN" };
            var database_datas = unitOfWork.OPDOutpatientExaminationNoteDataRepository.AsQueryable()
                          .Where(e => !e.IsDeleted && e.OPDOutpatientExaminationNoteId == this.OENId
                                 && code_option.Contains(e.Code)
                                 && e.Value != null && e.Value.ToLower().Equals("true")
                           ).ToArray();
            if (database_datas.Length == 1)
            {
                var obj_option = database_datas.FirstOrDefault();
                if (obj_option?.Code == "OPDOENYTDNGL")
                {
                    string[] index_codes = { "OPDOENTK0011ANS", "OPDOENTK0002ANS", "OPDOENTK0007ANS", "OPDOENTK0014ANS", "OPDOENTK0013ANS", "OPDOENTK0006ANS", "OPDOENTK0004ANS", "OPDOENTK0005ANS", "OPDOEN515", "OPDOEN517", "OPDOENTK0009ANS" };
                    return SetIndexByCode(datas, index_codes);
                }
                else
                    return datas;
            }
            return datas;            
        }
        private List<dynamic> SetIndexByCode(List<dynamic> curren_datas, string[] array_codes)
        {
            List<dynamic> result = new List<dynamic>();
            foreach(string code in array_codes)
            {
                var data = curren_datas.FirstOrDefault(e => e.Code == code);
                if (data != null)
                    result.Add(data);
            }
            return result;
        }

        public static string GetStringClinicCodeUsed(OPD visit)
        {
            List<string> list_result = new List<string>();
            var code_setup = visit?.Clinic?.SetUpClinicDatas;
            var data_form = visit.OPDOutpatientExaminationNote?.OPDOutpatientExaminationNoteDatas;
            if (visit?.OPDOutpatientExaminationNote?.Version == 1)
                return visit?.Clinic?.Code;

            if (data_form != null)
            {
                var codeInData = (from d in data_form
                                  where !d.IsDeleted
                                  && d.Code == "SETUPCLINICS"
                                  select d.Value).FirstOrDefault();
                if (!string.IsNullOrEmpty(codeInData))
                    return codeInData;
            }

            return code_setup;
        }

        private dynamic GetDataOENByVersion()
        {
            #region khám lâm sàn phiếu khám ngoại trú Version 1 
            if (this.Version == 1 || this.Version != 2) // cho chắc ăn
            {
                if (this.ClinicCode.Contains("FreeTextOnly"))
                {
                    string[] master_string = new string[] { };
                    if (this.ClinicCode.Contains("000") || this.ClinicCode.Contains("001"))
                        master_string = new string[] { "OPDOENKTTANS", "OPDOENKCKANS", "OPDOENKTPANS", "OPDOENKCBPKANS" };
                    else if (this.ClinicCode.Contains("002"))
                        master_string = new string[] { "OPDOENBMIANS", "OPDOENTUOI19", "OPDOENTUOI519", "OPDOENTUOI5", "OPDOENKTTTHGCANS", "OPDOENDGCGNMANS", "OPDOENDGDNMANS", "OPDOENKLNANS", "OPDOENKCBPKANS" };
                    else if (this.ClinicCode.Contains("003"))
                        master_string = new string[] { "OPDOENKTTANS", "OPDOENKTMANS", "OPDOENKHHANS", "OPDOENKCBPKANS" };
                    else if (this.ClinicCode.Contains("004"))
                        master_string = new string[] { "OPDOENNGNGANS", "OPDOENNTTTXHANS", "OPDOENHVTTCYANS", "OPDOENCGXGANS", "OPDOENKTTPVANS", "OPDOENMSTTLCTANS", "OPDOENCCQKANS" };
                    else if (this.ClinicCode.Contains("005"))
                        master_string = new string[] { "OPDOENKTTANS", "OPDOENKTMHANS", "OPDOENKTPANS", "OPDOENKCBPKANS", "OPDOENKDDANS", "OPDOENTCANS", "OPDOENPTVDANS", "OPDOENPTTTANS" };
                    else if (this.ClinicCode.Contains("006"))
                        master_string = new string[] { "OPDOENKTTANS", "OPDOENKCKANS", "OPDOENKTPANS", "OPDOENKCBPKANS", "OPDOENKDDANS", "OPDOENTCANS", "OPDOENPTVDANS", "OPDOENPTTTANS" };
                    else if (this.ClinicCode.Contains("007"))
                        master_string = new string[] { "OPDOENBHCANS", "OPDOENKYTANS", "OPDOENKCGTGANS", "OPDOENKTDANS", "OPDOENKCXANS", "OPDOENKHDANS", "OPDOENKCYANS", "OPDOENKTNANS", "OPDOENKTRTANS", "OPDOENTTLANS", "OPDOENKNKTANS" };
                    return GetOutPatientExaminationNoteDataFreeTextOnly(master_string);
                }
                else if (this.ClinicCode.Contains("RadioWithNull"))
                {
                    var options = new List<dynamic>();
                    if (this.ClinicCode.Contains("001"))
                        options = new List<dynamic>{
                        new { Code = "OPDOENNLHTENL", MasterString = new string[] {"OPDOENBHCANS", "OPDOENKYTANS", "OPDOENKCGTGANS","OPDOENKTDANS","OPDOENKCXANS","OPDOENKHDANS","OPDOENKCYANS","OPDOENKTNANS","OPDOENKTRTANS","OPDOENTTLANS"} },
                        new { Code = "OPDOENNLHTETE", MasterString = new string[] {"OPDOENNGNGANS","OPDOENNTTTXHANS","OPDOENHVTTCYANS","OPDOENCGXGANS","OPDOENKTTPVANS","OPDOENMSTTLCTANS"} },
                    };
                    return GetOutpatientExaminationNoteDataRadioWithNull(options);
                }
                else if (this.ClinicCode.Contains("RadioYesNo"))
                {
                    dynamic yes_option = null;
                    dynamic no_option = null;
                    if (this.ClinicCode.Contains("001"))
                    {
                        yes_option = new { Code = "OPDOENBNTHPTYES", MasterString = new string[] { "OPDOENKTTANS", "OPDOENKCKANS", "OPDOENKTP1ANS", "OPDOENKCBPKANS" } };
                        no_option = new { Code = "OPDOENBNTHPTNOO", MasterString = new string[] { "OPDOENKTTANS", "OPDOENKCKANS", "OPDOENKCBPKANS" } };
                    }
                    return GetOutpatientExaminationNoteDataRadioYesNo(yes_option, no_option);
                }
                else if (ClinicCode.Contains("MultiSelect"))
                {
                    var options = new List<dynamic>();
                    if (this.ClinicCode.Contains("001"))
                        options = new List<dynamic>{
                        new { Code = "OPDOENDGVD", MasterString = new string[] { "OPDOENKTTANS", "OPDOENMCNANS", "OPDOENDCTANS", "OPDOENTDTTANS", "OPDOENTHCVDANS", "OPDOENPHCNANS", "OPDOENKTPANS", "OPDOENKCBPKANS"} },
                        new { Code = "OPDOENDGTMHH", MasterString = new string[] {"OPDOENKTTANS","OPDOENCNHHANS","OPDOENSCDANS","OPDOENKNGSANS","OPDOENTDNTANS","OPDOENKCBPKANS"} },
                        new { Code = "OPDOENDGTK", MasterString = new string[] {"OPDOENKTTANS","OPDOENKNGTANS","OPDOENTTRLNANS","OPDOENNTKGANS","OPDOENDVTHANS","OPDOENDCDLANS","OPDOENKTPANS","OPDOENKCBPKANS"} },
                    };
                    else if (this.ClinicCode.Contains("002"))
                        options = new List<dynamic>{
                        new { Code = "OPDOENYTDKTT", MasterString = new string[] {
                            "OPDOENNGNGANS", "OPDOENNTTTXHANS", "OPDOENHVTTCYANS", "OPDOENCGXGANS", "OPDOENKTTPVANS",
                            "OPDOENMSTTLCTANS", "OPDOENCCQKANS"
                        } },
                        new { Code = "OPDOENYTDTRE", MasterString = new string[] {
                            "OPDOENTK0001ANS", "OPDOENTK0002ANS", "OPDOENTK0003ANS", "OPDOENTK0004ANS", "OPDOENTK0005ANS",
                            "OPDOENTK0006ANS", "OPDOENTK0007ANS", "OPDOENTK0008ANS", "OPDOENTK0009ANS", "OPDOENTK0010ANS"
                        } },
                        new { Code = "OPDOENYTDNGL", MasterString = new string[] {
                            "OPDOENTK0011ANS","OPDOENTK0012ANS","OPDOENTK0013ANS","OPDOENTK0014ANS", "OPDOENTK0015ANS",
                            "OPDOENTK0016ANS","OPDOENTK0017ANS","OPDOENTK0018ANS","OPDOENTK0019ANS", "OPDOENTK0027ANS"
                        } },
                        new { Code = "OPDOENYTDKBN", MasterString = new string[] {
                            "OPDOENTK0020ANS","OPDOENTK0021ANS","OPDOENTK0022ANS","OPDOENTK0023ANS","OPDOENTK0024ANS",
                            "OPDOENTK0025ANS","OPDOENTK0026ANS", "OPDOENTK0028ANS"
                        } },
                    };
                    return GetOutpatientExaminationNoteDataMultiSelect(options);
                }
                return GetOutPatientExaminationNoteDataFreeTextOnly(new string[] { "OPDOENCEFANS" });
            }
            #endregion
            #region Khám lâm sàn phiếu khám ngoại trú Version 2
            else if (this.Version == 2)
            {
                List<dynamic> result = new List<dynamic>();
                if (this.ClinicCode.Contains("FreeTextOnly"))
                {
                    List<string> master_string = new List<string>();
                    if (this.ClinicCode.Contains("FreeTextOnly-000"))
                        master_string.Add("OPDOENCEFANS");
                    if (this.ClinicCode.Contains("FreeTextOnly-001"))
                        master_string.AddRange(new string[] { "OPDOENKTTANS", "OPDOENKCKANS", "OPDOENKTP1ANS", "OPDOENKCBPKANS" });
                    if (this.ClinicCode.Contains("FreeTextOnly-002"))
                        master_string.AddRange(new string[] { "OPDOEN527", "OPDOENBMIANS", "OPDOENTUOI19", "OPDOENTUOI519", "OPDOENTUOI5", "OPDOENKTTTHGCANS", "OPDOENDGCGNMANS", "OPDOENDGDNMANS", "OPDOENKLNANS", "OPDOENKCBPKANS" });
                    if (this.ClinicCode.Contains("FreeTextOnly-003"))
                        master_string.AddRange(new string[] { "OPDOENKTTANS", "OPDOENKTMANS", "OPDOENKHHANS", "OPDOENKCBPKANS" });
                    if (this.ClinicCode.Contains("FreeTextOnly-004"))
                        master_string.AddRange(new string[] { "OPDOENNGNGANS", "OPDOENNTTTXHANS", "OPDOENHVTTCYANS", "OPDOENCGXGANS", "OPDOENKTTPVANS", "OPDOENMSTTLCTANS", "OPDOENCCQKANS" });
                    if (this.ClinicCode.Contains("FreeTextOnly-005"))
                        master_string.AddRange(new string[] { "OPDOENKTTANS", "OPDOENKCKANS", "OPDOENKTP1ANS", "OPDOENKCBPKANS", "OPDOENKDDANS", "OPDOENTCANS", "OPDOENPTVDANS", "OPDOENPTTTANS" }); // Cũ "OPDOENKTTANS", "OPDOENKTMHANS", "OPDOENKTPANS", "OPDOENKCBPKANS", "OPDOENKDDANS", "OPDOENTCANS", "OPDOENPTVDANS", "OPDOENPTTTANS"
                    if (this.ClinicCode.Contains("FreeTextOnly-006"))
                        master_string.AddRange(new string[] { "OPDOENKTTANS", "OPDOENKCKANS", "OPDOENKTPANS", "OPDOENKCBPKANS", "OPDOENKDDANS", "OPDOENTCANS", "OPDOENPTVDANS", "OPDOENPTTTANS" });
                    if (this.ClinicCode.Contains("FreeTextOnly-007"))
                        master_string.AddRange(new string[] { "OPDOENBHCANS", "OPDOENKYTANS", "OPDOENKCGTGANS", "OPDOENKTDANS", "OPDOENKCXANS", "OPDOENKHDANS", "OPDOENKCYANS", "OPDOENKTNANS", "OPDOENKTRTANS", "OPDOENTTLANS", "OPDOENKNKTANS" });

                    var list = GetOutPatientExaminationNoteDataFreeTextOnly(master_string.ToArray());
                    if (list != null)
                        result.AddRange(list);
                }
                if (this.ClinicCode.Contains("RadioWithNull"))
                {
                    var options = new List<dynamic>();
                    if (this.ClinicCode.Contains("RadioWithNull-001"))
                        options = new List<dynamic>{
                        new { Code = "OPDOENNLHTENL", MasterString = new string[] {"OPDOENBHCANS", "OPDOENKYTANS", "OPDOENKCGTGANS","OPDOENKTDANS","OPDOENKCXANS","OPDOENKHDANS","OPDOENKCYANS","OPDOENKTNANS","OPDOENKTRTANS","OPDOENTTLANS"} },
                        new { Code = "OPDOENNLHTETE", MasterString = new string[] {"OPDOENNGNGANS","OPDOENNTTTXHANS","OPDOENHVTTCYANS","OPDOENCGXGANS","OPDOENKTTPVANS","OPDOENMSTTLCTANS"} },
                    };
                    var list = GetOutpatientExaminationNoteDataRadioWithNull(options);
                    if (list != null && list.Count != 0)
                        result.AddRange(list);
                }
                if (this.ClinicCode.Contains("RadioYesNo"))
                {
                    dynamic yes_option = null;
                    dynamic no_option = null;
                    if (this.ClinicCode.Contains("RadioYesNo-001"))
                    {
                        yes_option = new { Code = "OPDOENBNTHPTYES", MasterString = new string[] { "OPDOENKTTANS", "OPDOENKCKANS", "OPDOENKTP1ANS", "OPDOENKCBPKANS" } };
                        no_option = new { Code = "OPDOENBNTHPTNOO", MasterString = new string[] { "OPDOENKTTANS", "OPDOENKCKANS", "OPDOENKCBPKANS" } };
                    }
                    var list = GetOutpatientExaminationNoteDataRadioYesNo(yes_option, no_option);
                    if (list != null && list.Count != 0)
                        result.AddRange(list);
                }
                if (ClinicCode.Contains("MultiSelect"))
                {
                    var options = new List<dynamic>();
                    if (this.ClinicCode.Contains("MultiSelect-001"))
                    {
                        options.AddRange(new List<dynamic>{
                        new { Code = "OPDOENDGVD", MasterString = new string[] { "OPDOENKTTANS", "OPDOENKCKANS", "OPDOENMCNANS", "OPDOENDCTANS", "OPDOENTDTTANS", "OPDOENTHCVDANS", "OPDOENPHCNANS", "OPDOENKTP1ANS", "OPDOENKCBPKANS"} },
                        new { Code = "OPDOENDGTMHH", MasterString = new string[] {"OPDOENKTTANS", "OPDOENKTP1ANS", "OPDOENKCKANS", "OPDOENCNHHANS","OPDOENSCDANS","OPDOENKNGSANS","OPDOENTDNTANS","OPDOENKCBPKANS"} },
                        new { Code = "OPDOENDGTK", MasterString = new string[] {"OPDOENKTTANS", "OPDOENKCKANS", "OPDOENKNGTANS","OPDOENTTRLNANS","OPDOENNTKGANS","OPDOENDVTHANS","OPDOENDCDLANS", "OPDOENKTP1ANS", "OPDOENKCBPKANS"} },
                        });
                        //mặc định vào mã MultiSelect-001 luôn có 4 trường này 
                        string[] dataDefault = { "OPDOENKTTANS", "OPDOENKCKANS", "OPDOENKTP1ANS", "OPDOENKCBPKANS" };
                        var data = GetOutPatientExaminationNoteDataFreeTextOnly(dataDefault);
                        if (data != null)
                            result.AddRange(data);
                    }
                    if (this.ClinicCode.Contains("MultiSelect-002"))
                        options.AddRange(new List<dynamic>{
                        new { Code = "OPDOENYTDKTT", MasterString = new string[] {
                            "OPDOENNGNGANS", "OPDOENNTTTXHANS", "OPDOENHVTTCYANS", "OPDOENCGXGANS", "OPDOENKTTPVANS",
                            "OPDOENMSTTLCTANS", "OPDOENCCQKANS"
                        } },
                        new { Code = "OPDOENYTDTRE", MasterString = new string[] {
                            "OPDOENTK0001ANS", "OPDOENTK0002ANS", "OPDOENTK0003ANS", "OPDOENTK0004ANS", "OPDOENTK0005ANS",
                            "OPDOENTK0006ANS", "OPDOENTK0007ANS", "OPDOENTK0008ANS", "OPDOENTK0009ANS", //"OPDOENTK0010ANS" khám tâm lý trẻ e thừa code này
                        } },
                        new { Code = "OPDOENYTDNGL", MasterString = new string[] {
                             "OPDOENTK0002ANS", "OPDOENTK0004ANS", "OPDOENTK0005ANS", "OPDOENTK0006ANS",  // Cũ "OPDOENTK0011ANS","OPDOENTK0012ANS","OPDOENTK0013ANS","OPDOENTK0014ANS", "OPDOENTK0015ANS", 
                             "OPDOENTK0007ANS", "OPDOENTK0009ANS", "OPDOENTK0011ANS", "OPDOENTK0014ANS",  // Cũ "OPDOENTK0016ANS","OPDOENTK0017ANS","OPDOENTK0018ANS","OPDOENTK0019ANS", "OPDOENTK0027ANS"
                             "OPDOENTK0013ANS", "OPDOEN515", "OPDOEN517"
                        } },
                        new { Code = "OPDOENYTDKBN", MasterString = new string[] {
                            "OPDOENTK0001ANS", "OPDOENTK0002ANS", "OPDOENTK0008ANS", "OPDOENTK0009ANS", // Cũ "OPDOENTK0020ANS","OPDOENTK0021ANS","OPDOENTK0022ANS","OPDOENTK0023ANS","OPDOENTK0024ANS",
                            "OPDOENTK0020ANS", "OPDOENTK0023ANS", "OPDOENTK0024ANS"                     // Cũ "OPDOENTK0025ANS","OPDOENTK0026ANS", "OPDOENTK0028ANS"
                        } },
                    });
                    var list = GetOutpatientExaminationNoteDataMultiSelect(options);
                    if (list != null && list.Count != 0)
                    {
                        list = SortData(list);
                        result.AddRange(list);
                    }    
                }
                return result.Distinct().ToList();
            }
            #endregion
            else return new List<dynamic>();
        }
    }
}