using System;
using System.Windows;
using WhiteboardApp.NetworkCommunicator;

namespace WhiteboardApp.Presentation
{
    /// <summary>
    /// Interaction logic for LogInPage.xaml
    /// </summary>
    public partial class LogInPage : Window
    {
        public LogInPage()
        {
            InitializeComponent();            
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var r = await ParticipantManager.SignIn();
            try
            {
                if (r)
                    this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot login! Exit program!");
                Environment.Exit(-1);
            }
            

        }
    }
}
