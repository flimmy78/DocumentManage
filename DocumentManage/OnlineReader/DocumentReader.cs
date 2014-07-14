using System;
using System.IO;
using System.Windows;
using DocumentManage.Views;
using Path = System.IO.Path;

namespace DocumentManage.OnlineReader
{
    public class DocumentReader : IDisposable
    {
        private static DocumentReaderBase DocReader;

        public static void Open(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                CustomMessageBox.Show("您要打开的文件不存在，请确认操作！");
                return;
            }
            Close();
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
            string fileType = Path.GetExtension(fileName).ToLower();
            switch (fileType)
            {
                case ".doc":
                case ".docx":
                    DocReader = new WordDocumentReader { FileName = fileName };
                    break;
                case ".xls":
                case ".xlsx":
                    DocReader = new ExcelDocumentReader { FileName = fileName };
                    break;
                case ".pdf":
                    DocReader = new PdfDocumentReader { FileName = fileName };
                    break;
                case ".dwg":
                    DocReader = new CadDocumentReader { FileName = fileName };
                    break;
                default:
                    DocReader = new UnknownDocumentReader { FileName = fileName };
                    break;
            }
            DocReader.Open();
            DocReader.Activate();
        }

        public static void Close()
        {
            if (DocReader != null)
                DocReader.Close();
        }

        public void Dispose()
        {
            if (DocReader != null)
            {
                DocReader.Close();
                DocReader.Dispose();
                DocReader = null;
            }
        }
    }
}
