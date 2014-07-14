using System;
using System.Runtime.InteropServices.Automation;

namespace DocumentManage.OnlineReader
{
    public class WordDocumentReader : DocumentReaderBase
    {
        public override void Open()
        {
            if (!ValidateFileName())
                return;

            ExternalApp = AutomationFactory.CreateObject("Word.Application");
            ExternalApp.Visible = true;
            ExternalApp.DocumentBeforePrint += CancelPrintEventHander;
            ExternalApp.DocumentBeforeSave += CancelSaveEventHander;
            ExternalApp.DisplayRecentFiles = false;
            ExternalApp.Options.AllowFastSave = false;
            ExternalApp.Options.DisplayPasteOptions = false;
            ExternalApp.Options.DisplaySmartTagButtons = false;
            ExternalApp.Options.SaveNormalPrompt = false;
            ExternalDoc = ExternalApp.Documents.Open(FileName, false, true, false);
            Activate();
            IsOpend = true;
        }

        public override void Activate()
        {
            if (ExternalApp != null)
            {
                ExternalApp.Activate();
            }
            if (ExternalDoc != null)
            {
                ExternalDoc.Activate();
            }
        }

        public override void Close()
        {
            try
            {
                if (ExternalDoc != null)
                {
                    ExternalDoc.Close();
                    ExternalDoc = null;
                }
                if (ExternalApp != null)
                {
                    ExternalApp.Quit(false);
                    ExternalApp = null;
                }
            }
            catch { }
            finally
            {
                ExternalDoc = null;
                GC.Collect();
                IsOpend = false;
            }
        }
    }
}
