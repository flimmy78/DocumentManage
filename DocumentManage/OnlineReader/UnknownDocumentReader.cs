using System;
using System.Runtime.InteropServices.Automation;
using DocumentManage.Views;

namespace DocumentManage.OnlineReader
{
    public class UnknownDocumentReader : DocumentReaderBase
    {
        public override void Open()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                using (dynamic shell = AutomationFactory.CreateObject("WScript.Shell"))
                {
                    try
                    {
                        shell.Run(FileName);
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show("打开文件失败！" + Environment.NewLine + ex.Message);
                    }
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
