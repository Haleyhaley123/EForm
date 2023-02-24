using Clients.E_Sign;
using DataAccess.Models.EDModel;
using DataAccess.Models.EIOModel;
using DataAccess.Models.EsignModel;
using DataAccess.Repository;
using EMRModels;
using Helper.Files;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Bussiness.ESign
{
    public class EsignBussiness
    {
        private IUnitOfWork _unitOfWork = new EfUnitOfWork();
        public bool SignWithType2(Guid BId)
        {
            //ký theo thứ tự setup esign(gửi email)
            var password = ConfigurationManager.AppSettings["ESIGN_USERNAME"].ToString();
            var username = ConfigurationManager.AppSettings["ESIGN_PASSWORD"].ToString();
            EsignModel request = new EsignModel();
            request.userName = username;
            request.passWord = password;
            request.typefollow = "2";
            request.userNameky = "thangdc3";
            request.followCode = "TEST_GCVED";
            request.tempmail_code = "B2B";
            request.b_dvi = "VINMEC_NEW";
            request.dataBase64 = "";
            var result = ESignRequest.SendRequet(BId, request);
            return result.code == "200";
        }
        public bool SignWithType1(Guid BId, string kind)
        {
            var password = ConfigurationManager.AppSettings["ESIGN_USERNAME"].ToString();
            var username = ConfigurationManager.AppSettings["ESIGN_PASSWORD"].ToString();
            // ký không theo thứ tự (gửi email)
            EsignModel request = new EsignModel();
            request.userName = username;
            request.passWord = password;
            request.typefollow = "1";
            request.userNameky = "thangdc3";
            request.tempmail_code = "B2B";
            request.b_dvi = "VINMEC_NEW";
            request.b_vaitro = "TRUONGKHOA";
            request.typeSign = "POSITION";
            request.withImg = "6";
            request.pageIndex = "0";
            request.heightImg = "3";
            request.bottompos = "-3";
            request.locationKey = kind;
            request.dataBase64 = "";
            var result = ESignRequest.SendRequet(BId, request);
            return result.code == "200";
        }
        public bool SignWithType6(Guid BId, string username, string password, string kind, Guid FormId, Guid VisitId, string VisitType)
        {
            // ký luôn
            EsignModel request = new EsignModel();
            request.userName = username;
            request.passWord = password;
            request.typefollow = "6";
            request.b_vaitro = "TRUONGKHOA";
            request.b_dvi = "VINMEC_NEW";
            request.typeSign = "POSITION";
            request.withImg = "6";
            request.heightImg = "3";
            request.bottompos = "-3";
            request.pageIndex = "0";
            request.sw = "EMR";
            request.dataBase64 = "";
            request.locationKey = kind;
            request.userNameky = "";
            request.followCode = "";
            request.link_callback = "";
            request.signobj = "";
            request.tempmail_code = "";
            request.invisible = "2";
            var result = ESignRequest.SendRequet(BId, request);
            if (result.code == "200")
            {
                CreateEMRDocument(VisitId, FormId, VisitType, result.base64);
            }
            return result.code == "200";
        }
        public Guid ESignInt(Guid FormId, Guid VisitId, string VisitType, string FormCode, Dictionary<string, string> data = null)
        {
            var checkEsign = _unitOfWork.EsignRepository.FirstOrDefault(e => !e.IsDeleted && e.FormId == FormId);
            if (checkEsign == null)
            {
                var esign = new Esigns
                {
                    VisitId = VisitId,
                    FormId = FormId,
                    FormCode = FormCode
                };
                _unitOfWork.EsignRepository.Add(esign);
                _unitOfWork.Commit();
                var esignDoc = CreateEsignDocument(esign.Id, FormCode, VisitType, data);
                //var emrDoc = CreateEMRDocument(VisitId, FormId, VisitType, esignDoc.base64);
                return esign.Id;
            }
            return checkEsign.Id;
        }
        private EsignResponse CreateEsignDocument(Guid BId, string FormCode, string VisitType, Dictionary<string, string> data = null)
        {
            DataToWord dataword = new DataToWord(BId.ToString(), GetFullFilePath(FormCode, VisitType));
            dataword.SaveData(data, FormCode);
            // khơi tạo tài liệu với typeflow = 5
            // base64, b_id, link_callback, username, password (admin account)
            //
            var url_pdf = dataword.SaveData(data, FormCode);
            Byte[] bytespdf = File.ReadAllBytes(url_pdf);
            String base64_file = Convert.ToBase64String(bytespdf);
            var link_callback = "";
            var password = "Vietnam@2468";
            var username = "vm.test2";
            EsignModel request = new EsignModel();
            request.userName = username;
            request.passWord = password;
            request.link_callback = link_callback;
            request.dataBase64 = base64_file;
            request.b_dvi = "VINMEC_NEW";
            request.typefollow = "5";
            request.invisible = "2";

            return ESignRequest.SendRequet(BId, request);
        }
        public Guid CreateEMRDocument(Guid visitId, Guid formId, string visitTypeCode, string data)
        {
            byte[] sPDFDecoded = Convert.FromBase64String(data);
            var upload_path = ConfigurationManager.AppSettings["FilePath"];
            var date_now = DateTime.Now.ToString("dd-MM-yyyy");
            var folder_virtual_path = $"/UploadFiles/Images/Temp/{date_now}";
            var folder_physic_path = $"{upload_path}{folder_virtual_path}";
            bool exists = Directory.Exists(folder_physic_path);
            if (!exists)
                Directory.CreateDirectory(folder_physic_path);
            string file_name = $"{Guid.NewGuid().ToString()}.pdf";
            string file_virtual_path = $"{folder_virtual_path}/{file_name}";
            string file_physic_path = $"{upload_path}{file_virtual_path}";
            File.WriteAllBytes(file_physic_path, sPDFDecoded);
            var file_data = new UploadImage
            {
                Name = "Ký từ esign",
                Title = "ESign",
                Path = file_physic_path,
                Url = file_virtual_path,
                FileType = "pdf",
                VisitType = visitTypeCode,
                VisitId = visitId,
                FormId = formId,
            };
            _unitOfWork.UploadImageRepository.Add(file_data);
            _unitOfWork.Commit();
            return file_data.Id;
        }
        public string GetFullFilePath(string FormCode, string VisitType)
        {
            string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("Documents"));
            return string.Format("{0}\\{1}_{2}.docx", FilePath, VisitType, FormCode);
        }
    }
}
