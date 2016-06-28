using System;
using System.Collections.Generic;
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

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for PostItColorPalette.xaml
    /// </summary>
    public delegate void ColorPickedEvent(Control callingControl, string colorCode);
    public partial class PostItColorPalette : UserControl
    {
        private Control _callingControl = null;
        public event ColorPickedEvent colorPickedEventHandler;
        public Control CallingControl
        {
            get { return _callingControl; }
            set { _callingControl = value; }
        }
        public PostItColorPalette()
        {
            InitializeComponent();
        }
        private void ColorItem_Click(object sender, RoutedEventArgs e)
        {
            if (colorPickedEventHandler != null)
            {
                string colorCode = (string)(sender as PieInTheSky.PieMenuItem).Tag;
                colorPickedEventHandler(_callingControl, colorCode);
            }
        }

        private void ColorItem_fbad6b_Click(object sender, RoutedEventArgs e)
        {
            if (colorPickedEventHandler != null)
            {
                string colorCode = (string)(sender as PieInTheSky.PieMenuItem).Tag;
                colorPickedEventHandler(_callingControl, colorCode);
            }
        }
        
    }
}
