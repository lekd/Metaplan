using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace PostIt_Prototype_1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
    }
}