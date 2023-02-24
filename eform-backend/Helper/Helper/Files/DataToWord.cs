using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
namespace Helper.Files
{
    public class DataToWord
    {
        private string FileId;
        private string TemplatePath;
        private string TmpPath = "Tmp";
        private string TmpFilePath;
        private string ZipFilePath;
        private string DocFilePath;
        private string PDFFilePath;
        private string Tmp;
        private string TmpDirectory;
        public DataToWord(string FileId, string TemplatePath)
        {
            this.FileId = FileId;
            this.Tmp = Guid.NewGuid().ToString();
            this.TemplatePath = TemplatePath;
            this.TmpDirectory = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, string.Format("{0}\\{1}", TmpPath, Tmp));

            System.IO.Directory.CreateDirectory(this.TmpDirectory);

            this.TmpFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, string.Format("{0}\\{1}", TmpDirectory, FileId));
            this.ZipFilePath = string.Format("{0}.{1}", TmpFilePath, "zip");
            this.DocFilePath = string.Format("{0}.{1}", TmpFilePath, "docx");
            this.PDFFilePath = string.Format("{0}.{1}", TmpFilePath, "pdf");
            this.MoveTemplateToTmpPath();
            this.UnzipFile();
        }
        private void MoveTemplateToTmpPath()
        {
            File.Copy(TemplatePath, ZipFilePath, true);
        }
        private void UnzipFile()
        {
            ZipFiles.ExtractToDirectory(ZipFilePath, TmpFilePath);
        }
        private void ZipFileToWord()
        {
            ZipFiles.CreateFromDirectory(TmpFilePath, ZipFilePath);
            File.Copy(ZipFilePath, DocFilePath, true);
        }
        public string SaveData(Dictionary<string, string> finAndRelpaces, string formcode)
        {
            //System.Xml.XmlDocument wordDoc = new System.Xml.XmlDocument();
            //var file = string.Format("{0}/word/document.xml", TmpFilePath);
            //wordDoc.Load(file);
            //wordDoc.Save(file);
            var file = string.Format("{0}\\word\\document.xml", TmpFilePath);
            string text = File.ReadAllText(file);
            foreach (KeyValuePair<string, string> item in finAndRelpaces)
            {
                if (item.Key == "[PHONESITE]") continue;
                text = text.Replace(item.Key, item.Value);
            }
            File.WriteAllText(file, text);
            //thay footer
            if(formcode == "A01_145_050919_VE")
            {
                var footer = string.Format("{0}\\word\\footer1.xml", TmpFilePath);
                string fotter_text = File.ReadAllText(footer);
                foreach (KeyValuePair<string, string> item in finAndRelpaces)
                {
                    if (item.Key != "[PHONESITE]") continue;
                    fotter_text = fotter_text.Replace(item.Key, item.Value);
                }
                File.WriteAllText(footer, fotter_text);
            }
            ZipFileToWord();
            var file_path_doc = TmpFilePath + ".docx";
            var file_path_pdf = WordToPdf(file_path_doc);
            return file_path_pdf;
        }
        public string GetDocPath()
        {
            return this.DocFilePath;
        }
        public string WordToPdf(string filepath)
        {
          Application application = new Application();
            Document document = null;
            try
            {
                application.Visible = false;
                document = application.Documents.Open(filepath);
                var url_pdf = filepath.Split('.')[0];
                var ull_pdf = url_pdf + ".pdf";
                document.ExportAsFixedFormat(ull_pdf, WdExportFormat.wdExportFormatPDF);

                //WdStatistic stat = WdStatistic.wdStatisticPages;
                //object missing = System.Reflection.Missing.Value;
                //int num = document.ComputeStatistics(stat, ref missing);  
                return ull_pdf;
            }
            catch (Exception e)
            {
                return null;
            }
            finally
            {
                document.Close();
                application.Quit();
            }
            return null;
        }
        public string GetBase64(string type = "docx")
        {
            Byte[] bytes = File.ReadAllBytes(type == "docx" ? this.DocFilePath : this.PDFFilePath);
            return Convert.ToBase64String(bytes);
        }
    }
}
