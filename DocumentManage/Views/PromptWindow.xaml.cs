using System;
using System.Windows;
using System.Windows.Controls;

namespace DocumentManage.Views
{
    public partial class PromptWindow : ChildWindow
    {
        private EventHandler onOkEventHandler;
        public PromptWindow()
        {
            InitializeComponent();
        }

        public event EventHandler OnOk
        {
            add { onOkEventHandler += value; }
            remove
            {
                if (onOkEventHandler != null)
                    onOkEventHandler -= value;
            }
        }

        public string InputText
        {
            get { return txtInputText.Text; }
        }

        public void SetTitle(string strTitle)
        {
            this.Title = strTitle;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (onOkEventHandler != null)
            {
                onOkEventHandler(this, null);
            }
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

