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
    public delegate void SelectedColorApproved(object sender,Control callingControl,string approvedColorCode);
    public partial class PostItColorPalette : UserControl
    {
        private Control _callingControl = null;
        public event ColorPickedEvent ColorPickedEventHandler;
        public event SelectedColorApproved SelectedColorApprovedHandler;
        public Control CallingControl
        {
            get { return _callingControl; }
            set { _callingControl = value; }
        }
        public PostItColorPalette()
        {
            InitializeComponent();
        }
        public void SetSize(double w, double h)
        {
            var diagonal = (float)Math.Sqrt(w * w + h * h);
            ColorMenu.Radius = diagonal/2 + 30;
            ColorMenu.InnerRadius = ColorMenu.Radius - 30;
            this.Width = ColorMenu.Radius*2;
            this.Height = ColorMenu.Radius * 2; ;
            this.UpdateLayout();
        }
        public string SelectedColorCode = "";
        private void ColorItem_Click(object sender, RoutedEventArgs e)
        {
            var colorCode = (string)(sender as PieInTheSky.PieMenuItem).Tag;
            SelectedColorCode = colorCode;
            if (ColorPickedEventHandler != null)
            {
                ColorPickedEventHandler(_callingControl, colorCode);
            }
        }

        private void btn_CenterPalette_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedColorApprovedHandler != null)
            {
                SelectedColorApprovedHandler(this,_callingControl,SelectedColorCode);
            }
        }

        
    }
}
