using Common;
using DataAccess.Models;
using DataAccess.Models.GeneralModel;
using DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Helper
{
    public class HandleApiILog
    {
        // protected static IUnitOfWork unitOfWork = new EfUnitOfWork();
        public static void Success(string url)
        {
        }
        public static void ApiGwInfo(string url, DateTime start_time, DateTime end_time, string response)
        {
            using (IUnitOfWork unitOfWork = new EfUnitOfWork())
            {
                unitOfWork.LogTmpRepository.Add(new LogTmp()
                {
                    Id = Guid.NewGuid(),
                    Ip = ConfigurationManager.AppSettings["APP_ID"] != null ? ConfigurationManager.AppSettings["APP_ID"].ToString() : "NONE",
                    Action = "APIGW",
                    URI = CutLengString(url),
                    CreatedAt = start_time,
                    UpdatedAt = end_time,
                    Response = response
                });
                unitOfWork.Commit();
            }
        }
        public static void Error(string url, string error)
        {
            using (IUnitOfWork unitOfWork = new EfUnitOfWork())
            {
                var app_id = ConfigurationManager.AppSettings["APP_ID"];
                try
                {
                    var scope = url.Split('?')[0];
                    var noti = new SystemNotification
                    {
                        Id = Guid.NewGuid(),
                        Service = Constants.SERVICE_APIGW,
                        Scope = scope,
                        Subject = url,
                        Content = (app_id != null ? app_id.ToString() : "NONE") + ": " + error,

                    };
                    unitOfWork.SystemNotificationRepository.Add(noti);
                    unitOfWork.Commit();
                }
                catch { }
            }
        }
        public static void ErrorToTmpLog(string t, string url = null)
        {
            using (IUnitOfWork unitOfWork = new EfUnitOfWork())
            {
                LogTmp log = new LogTmp
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    Ip = "",
                    URI = CutLengString(url),
                    Action = "ERROR",
                    Request = t,
                    Response = "DONE"
                };
                unitOfWork.LogTmpRepository.Add(log);
                unitOfWork.Commit();
            }
        }
        public static void InfoToTmpLog(LogTmp log)
        {
            using (IUnitOfWork unitOfWork = new EfUnitOfWork())
            {
                unitOfWork.LogTmpRepository.Add(log);
                unitOfWork.Commit();
            }
        }
        public static void ESignToTmpLog(string userName,
                    string passWord,
                    string b_dvi,
                    string b_vaitro,
                    string b_Id,
                    string dataBase64,
                    string imageSignBase64,
                    string type,
                    string typeSign,
                    string locationKey,
                    string positionX,
                    string positionY,
                    string withImg,
                    string heightImg,
                    string pageIndex,
                    string bottompos,
                    string sw,
                    string typefollow,
                    string userNameky,
                    string endfollow,
                    string followCode,
                    string link_callback,
                    string signobj,
                    string tempmail_code,
                    string invisible, string rep)
        {
            dynamic rawdata = new
            {
                userName,
                passWord = "******",
                b_dvi,
                b_vaitro,
                b_Id,
                dataBase64,
                imageSignBase64,
                type,
                typeSign,
                locationKey,
                positionX,
                positionY,
                withImg,
                heightImg,
                pageIndex,
                bottompos,
                sw,
                typefollow,
                userNameky,
                endfollow,
                followCode,
                link_callback,
                signobj,
                tempmail_code,
                invisible
            };
            var loger = new LogTmp()
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Ip = "",
                URI = "",
                Action = "ESIGN_REQUEST",
                Request = new JavaScriptSerializer().Serialize(rawdata),
                Response = rep
            };
            using (IUnitOfWork unitOfWork = new EfUnitOfWork())
            {
                unitOfWork.LogTmpRepository.Add(loger);
                unitOfWork.Commit();
            }
        }

        private static string CutLengString(string url)
        {
            if (url == null) return null;
            if (url.Length <= 450) return url;
            return url.Substring(0, 450);
        }
    }
}
