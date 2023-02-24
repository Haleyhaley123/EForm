using DataAccess.Models;
using DataAccess.Repository;
using System;
using System.Net;
using System.Web;
namespace EForm.Common
{
    public class CustomLog
    {
        public static readonly NLog.Logger accesslog = NLog.LogManager.GetLogger("accesslogger");
        public static readonly NLog.Logger errorlog = NLog.LogManager.GetLogger("errorlogger");
        public static readonly NLog.Logger apigwlog = NLog.LogManager.GetLogger("apigwlogger");
        public static readonly NLog.Logger intervaljoblog = NLog.LogManager.GetLogger("intervaljoblogger");
        public static readonly NLog.Logger apipubliblog = NLog.LogManager.GetLogger("apipubliblog");
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
                    Response = "DONE"
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
                    Response = "DONE"
                };
                unitOfWork.LogTmpRepository.Add(log);
                unitOfWork.Commit();
            }
        }
        public static void Info(LogTmp log)
        {
            using (IUnitOfWork unitOfWork = new EfUnitOfWork())
            {
                unitOfWork.LogTmpRepository.Add(log);
                unitOfWork.Commit();
            }
        }
        private static string GetIpAddress()
        {
            try
            {
                string strHostName = Dns.GetHostName();
                IPHostEntry ipHostInfo = Dns.GetHostEntry(strHostName);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                return ipAddress.ToString();
            }
            catch
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