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

namespace WhiteboardApp.Presentation
{
    /// <summary>
    /// Interaction logic for PostItColorPalette.xaml
    /// </summary>
    public delegate void ColorPickedEvent(Control callingControl, string colorCode);
    public delegate void SelectedColorApproved(object sender,Control callingControl,string approvedColorCode);
    public partial class PostItColorPalette : UserControl
    {
        private Control _callingControl = null;
        public event ColorPickedEvent colorPickedEventHandler;
        public event SelectedColorApproved selectedColorApprovedHandler;
        public Control CallingControl
        {
            get { return _callingControl; }
            set { _callingControl = value; }
        }
        public PostItColorPalette()
        {
            InitializeComponent();
        }
        public void setSize(double w, double h)
        {
            var diagonal = (float)Math.Sqrt(w * w + h * h);
            ColorMenu.Radius = diagonal/2 + 30;
            ColorMenu.InnerRadius = ColorMenu.Radius - 30;
            this.Width = ColorMenu.Radius*2;
            this.Height = ColorMenu.Radius * 2; ;
            this.UpdateLayout();
        }
        public string selectedColorCode = "";
        private void ColorItem_Click(object sender, RoutedEventArgs e)
        {
            var colorCode = (string)(sender as PieInTheSky.PieMenuItem).Tag;
            selectedColorCode = colorCode;
            if (colorPickedEventHandler != null)
            {
                colorPickedEventHandler(_callingControl, colorCode);
            }
        }

        private void btn_CenterPalette_Click(object sender, RoutedEventArgs e)
        {
            if (selectedColorApprovedHandler != null)
            {
                selectedColorApprovedHandler(this,_callingControl,selectedColorCode);
            }
        }

        
    }
}
