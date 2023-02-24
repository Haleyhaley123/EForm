using EForm.Common;
using Helper.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EForm.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //XmlDocument wordDoc = new XmlDocument();
            //var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.xml");
            //wordDoc.Load(file);

            //XmlNodeList mergeNodes = wordDoc.SelectNodes("//MergeField");

            //foreach (XmlNode mergeNode in mergeNodes)
            //{
            //    string fieldName = mergeNode.Attributes["name"].Value;
            //    // Do something here based on field name
            //    // e.g.:

            //    mergeNode.InnerText = "Mail Merge là chức năng soạn thư hàng loạt trong Microsoft Word, được nhiều người gọi bằng thuật ngữ trộn thư. Với Mail Merge, bạn có thể tạo cùng lúc nhiều thư với cùng nội dung nhưng khác nhau ở một vài thông tin. Để bắt đầu sử dụng Mail Merge, bạn cần chuẩn bị nội dung cơ bản và nguồn dữ liệu (data source). Cụ thể: Nguồn dữ liệu: thể hiện những nội dung tùy biến khác nhau giữa các bức thư sau khi tạo ra hàng loạt từ Mail Merge. Những chi tiết tùy biến này có thể là họ tên, địa chỉ người nhận, tên doanh nghiệp, tổ chức… và thường trình bày trong một danh sách dữ liệu. Danh sách này thường là danh bạ (contacts) trong Microsoft Outlook hoặc được trình bày trong một bảng tính Excel, một cơ sở dữ liệu trong Access, một table trong Word 2010 hay văn bản bất kỳ có cấu trúc phân loại dữ liệu thật rõ ràng. Nội dung cơ bản: là những phần giống nhau giữa các thư, chẳng hạn tên và địa chỉ người gửi trên phong bì, nội dung chính của lá thư, email hay một phiếu ưu đãi. Nội dung cơ bản thường đi cùng các vị trí định sẵn (Place Holder), là nơi thể hiện những chi tiết thay đổi lấy từ nguồn dữ liệu. Mỗi chi tiết được gọi là trường (Merge Field), được trình bày dưới dạng «tên field».";
            //}

            //wordDoc.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test2.docx"));

            //var dic = new Dictionary<string, string>();

            //dic.Add("xxx", "newvalue");
            //var fileId = Guid.NewGuid().ToString();
            //var template_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.docx");
            //var data2Word = new DataToWord(fileId, template_path);
            //data2Word.SaveData(dic);
            //var docpath = data2Word.GetDocPath();
            //var pdfpath = data2Word.GetPdfPath();
            //var base64 = data2Word.GetBase64();

            try
            
            {
                var form_token = Request.Headers.GetValues("RequestVerificationToken").First();
                var cookie_token = Request.Cookies["__RequestVerificationToken"].ToString();
                var csrf_token = string.Format("{0}{1}", form_token, cookie_token);
                //CachedSession.StoreInBlackList(csrf_token);
            }
            catch (Exception) { }
            ViewBag.Title = "Home Page";
            var token = CSRFToken.Generate();
            ViewBag.CSRFToken = token.Substring(0, 60);
            HttpCookie cookie = new HttpCookie("__RequestVerificationToken", token.Substring(60));
            cookie.Expires = DateTime.Now.AddMinutes(1440);
            Response.Cookies.Add(cookie);
            return View();
        }
    }
}
