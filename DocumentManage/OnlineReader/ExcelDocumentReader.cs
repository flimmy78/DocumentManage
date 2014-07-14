using System;
using System.Runtime.InteropServices.Automation;

namespace DocumentManage.OnlineReader
{
    public class ExcelDocumentReader : DocumentReaderBase
    {
        public override void Open()
        {
            if (!ValidateFileName())
                return;

            ExternalApp = AutomationFactory.CreateObject("Excel.Application");
            ExternalApp.Visible = true;
            ExternalApp.WorkbookBeforePrint += CancelPrintEventHander;
            ExternalApp.WorkbookBeforeSave += CancelSaveEventHander;
            ExternalApp.DisplayRecentFiles = false;
            ExternalApp.DisplayClipboardWindow = false;
            ExternalDoc = ExternalApp.Workbooks.Open(FileName, 0, true);
            IsOpend = true;
        }

        public override void Close()
        {
            try
            {
                if (ExternalDoc != null)
                    ExternalDoc.Close();

                if (ExternalApp != null)
                {
                    ExternalApp.Quit(false);
                    ExternalApp = null;
                }
            }
            catch
            {
            }
            finally
            {
                ExternalDoc = null;
                GC.Collect();
                IsOpend = false;
            }
        }

        public override void Activate()
        {
            if (ExternalDoc != null)
            {
                ExternalDoc.Activate();
            }
        }
    }
}
