using System;
using System.Runtime.InteropServices.Automation;

namespace DocumentManage.OnlineReader
{
    public class PdfDocumentReader : DocumentReaderBase
    {
        public override void Open()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                using (dynamic shell = AutomationFactory.CreateObject("WScript.Shell"))
                {
                    shell.Run(FileName);
                }
            }
        }

        public override void Close()
        {
        }

        public override void Activate()
        {
        }
    }
}
