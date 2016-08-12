using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web.Hosting;
using System.Windows;
using System.Windows.Threading;

namespace WhiteboardApp
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // TODO: Remove for production
#if DEBUG
            ServicePointManager.ServerCertificateValidationCallback +=
                (asender, cert, chain, sslPolicyErrors) => true;
#endif

            Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Dispatcher.UnhandledException += DispatcherOnUnhandledException;
        }

        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception, "DispatcherOnUnhandledException");
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception, "CurrentDomainOnUnhandledException");
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception, "CurrentOnDispatcherUnhandledException");
        }

        private void HandleException(Exception e, string s)
        {
            MessageBox.Show(e.ToString() + "\r\n->"+s);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

        }
    }
}