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
    /// Interaction logic for RemotePointerUI.xaml
    /// </summary>
    public partial class RemotePointerUI : UserControl
    {
        int _pointerID;

        public int PointerID
        {
            get { return _pointerID; }
            set { _pointerID = value; }
        }

        public RemotePointerUI()
        {
            InitializeComponent();
        }
        public void setPointerColor(string colorCode)
        {
            //change display color of the pointer to differentiate it from the previous ones
            PointerHeart.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(colorCode));
        }
    }
}
