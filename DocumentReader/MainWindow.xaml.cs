using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DocumentReader
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "EXCEL|*.xls;*.xlsx|所有文件|*.*";
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ctrlOffice1.FileName = dialog.FileName;
                ctrlOffice1.Open();
            }

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            ctrlOffice1.Close();
        }
    }
}
