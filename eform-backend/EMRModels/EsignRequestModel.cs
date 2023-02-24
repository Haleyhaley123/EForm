using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace EMRModels
{
    public class EsignResponse
    {
        public string b_id { get; set; }
        public string base64 { get; set; }
        public string code { get; set; }
    }
    public class EsignRequestModel
    {
        List<ConfirmData> Datas { get; set; }
        public string FormCode { get; set; }
        public Guid FormId { get; set; }

    }
    public class ConfirmData
    {
        public string Code { get; set; }
        public string Value { get; set; }
    }
    public class StatusModel
    {
        public string status { get; set; }
        public string data { get; set; }
    }
    public class CancelSignModel
    {
        public List<string> anyType { get; set; }
    }
    public class StatusResultModel
    {
        public string TKHKSO { get; set; }
        public string NGGIKSO { get; set; }
    }
    public class StatusRequestModel
    {
        public string userName { get; set; }
        public string passWord { get; set; }
    }
    public class EsignModel
    {
        public string userName { get; set; }
        public string passWord { get; set; }
        public string b_dvi { get; set; }
        public string b_vaitro { get; set; }
        public string b_Id { get; set; }
        public string imageSignBase64 { get; set; }
        public string dataBase64 { get; set; }
        public string type { get; set; }
        public string typeSign { get; set; }
        public string locationKey { get; set; }
        public string positionX { get; set; }
        public string positionY { get; set; }
        public string withImg { get; set; }
        public string heightImg { get; set; }
        public string pageIndex { get; set; }
        public string bottompos { get; set; }
        public string sw { get; set; }
        public string typefollow { get; set; }
        public string userNameky { get; set; }
        public string endfollow { get; set; }
        public string followCode { get; set; }
        public string link_callback { get; set; }
        public string urlfile { get; set; }
        public string signobj { get; set; }
        public string tempmail_code { get; set; }
        public string invisible { get; set; }
        public string ConfirmType { get; set; }
    }
    public class ListFormPIDModel
    {
        public string PID { get; set; }
        public string FullName { get; set; }
        public List<ListFormSpecsModel> Specs { get; set; }
    }    
    public class ListFormSpecsModel
    {
        public string NameSpec { get; set; }
        public List<ListFormModel> Profiles { get; set; }  

    }
    public class ListFormModel
    {
        public string ViName { get; set; }
        public string FormCode { get; set; }
        public string TypeName { get; set; }
        public Guid? VisitId { get; set; }
        public Guid? FormId { get; set; }
        public int Confirm { get; set; }
    }
}