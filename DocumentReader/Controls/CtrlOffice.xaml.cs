using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Office.Core;
using Excel= Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace DocumentReader.Controls
{
    /// <summary>
    /// CtrlWord.xaml 的交互逻辑
    /// </summary>
    public partial class CtrlOffice : UserControl, IReaderControl
    {
        Excel.Application app = new Excel.Application();
        public CtrlOffice()
        {
            InitializeComponent();
        }

        public string FileName { get; set; }

        public void Open()
        {
            if (string.IsNullOrEmpty(FileName))
                return;
            OpenExcel();
        }

        private void OpenExcel()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                MessageBox.Show("文件路径为空，不能打开文件！");
                return;
            }
            if (!File.Exists(FileName))
            {
                MessageBox.Show("您要打开的文件不存在！");
                return;
            }
            app.DisplayFullScreen = true;
            app.DisplayFunctionToolTips = false;
            app.DisplayFormulaBar = false;
            app.Workbooks.Open(FileName);
        }

        public void Close()
        {
            app.Workbooks.Close();
            app.Quit();
        }
    }
}
