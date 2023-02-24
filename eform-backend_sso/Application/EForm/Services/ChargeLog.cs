using DataAccess.Models;
using DataAccess.Repository;
using System;
using System.Net;
using System.Web;

namespace EForm.Services
{
    public class ChargeLog
    {
        public static void Error(string t)
        {
            using (IUnitOfWork unitOfWork = new EfUnitOfWork())
            {
                LogTmp log = new LogTmp
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    Ip = GetIpAddress(),
                    URI = GetUrl(),
                    Action = "ERROR",
                    Request = t,
                    Response = "DONE",
                };
                unitOfWork.LogTmpRepository.Add(log);
                unitOfWork.Commit();
            }
        }
        public static void Info(string t)
        {
            using (IUnitOfWork unitOfWork = new EfUnitOfWork())
            {
                LogTmp log = new LogTmp
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    Ip = GetIpAddress(),
                    URI = GetUrl(),
                    Action = "INFO",
                    Request = t,
                    Response = "DONE",
                };
                unitOfWork.LogTmpRepository.Add(log);
                unitOfWork.Commit();
            }
        }
        private static string GetIpAddress()
        {
            try {
                string strHostName = Dns.GetHostName();
                IPHostEntry ipHostInfo = Dns.GetHostEntry(strHostName);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                return ipAddress.ToString();
            } catch
            {
                return "";
            }
        }
        private static string GetUrl()
        {
            try
            {
                return HttpContext.Current.Request.Url.AbsoluteUri;
            }
            catch
            {
                return "";
            }
        }
    }
}