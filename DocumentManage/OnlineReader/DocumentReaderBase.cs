using System;
using System.IO;

namespace DocumentManage.OnlineReader
{
    public delegate void DocumentBeforePrintEventHandler(dynamic sender, ref bool cancel);
    public delegate void DocumentBeforeSaveEventHandler(dynamic sender, bool saveas, ref bool cancel);
    public abstract class DocumentReaderBase : IDisposable
    {
        protected static DocumentBeforePrintEventHandler CancelPrintEventHander;
        protected static DocumentBeforeSaveEventHandler CancelSaveEventHander;

        protected dynamic ExternalApp;
        protected dynamic ExternalDoc;

        public bool IsOpend { get; set; }

        static DocumentReaderBase()
        {
            CancelPrintEventHander += CancelUserOperation;
            CancelSaveEventHander += CancelUserOperation;
        }

        private static void CancelUserOperation(object sender, ref bool cancel)
        {
            cancel = true;
        }

        private static void CancelUserOperation(object sender, bool saveas, ref bool cancel)
        {
            cancel = true;
        }

        public string FileName { get; set; }

        public abstract void Open();

        public abstract void Close();

        public abstract void Activate();

        protected bool ValidateFileName()
        {
            return !string.IsNullOrEmpty(FileName) && File.Exists(FileName);
        }

        public void Dispose()
        {
            Close();
            IsOpend = false;
        }
    }
}
