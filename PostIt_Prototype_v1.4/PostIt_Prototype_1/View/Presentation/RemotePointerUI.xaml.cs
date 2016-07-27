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
    /// Interaction logic for RemotePointerUI.xaml
    /// </summary>
    public partial class RemotePointerUi : UserControl
    {
        int _pointerId;

        public int PointerId
        {
            get { return _pointerId; }
            set { _pointerId = value; }
        }

        public RemotePointerUi()
        {
            InitializeComponent();
        }
        public void SetPointerColor(string colorCode)
        {
            //change display color of the pointer to differentiate it from the previous ones
            PointerHeart.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(colorCode));
        }
    }
}
