using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace DocumentReader
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length < 1)
            {

            }
        }
    }
}
