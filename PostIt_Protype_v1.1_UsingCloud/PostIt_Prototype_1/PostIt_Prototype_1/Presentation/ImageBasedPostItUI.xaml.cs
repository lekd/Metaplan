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
using System.Drawing;
using PostIt_Prototype_1.PostItObjects;

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for ImageBasedPostItUI.xaml
    /// </summary>
    public partial class ImageBasedPostItUI : UserControl, IPostItUI
    {
        int _noteID;
        int _initWidth = 200;

        public int InitWidth
        {
            get { return _initWidth; }
            set { _initWidth = value; }
        }
        int _initHeight = 200;

        public int InitHeight
        {
            get { return _initHeight; }
            set { _initHeight = value; }
        }
        public int getNoteID()
        {
            return _noteID;
        }
        public void setNoteID(int id)
        {
            _noteID = id;
        }
        float _left;

        public float Left
        {
            get { return _left; }
            set { _left = value; }
        }
        float _top;

        public float Top
        {
            get { return _top; }
            set { _top = value; }
        }
        public ImageBasedPostItUI()
        {
            InitializeComponent();
        }
        public void updateDisplayedContent(object content)
        {
            if (content == null)
            {
                return;
            }
            //contentDisplayer.Source = (BitmapImage)content;
            Bitmap bmp = (Bitmap)content;
            _initWidth = bmp.Width;
            _initHeight = bmp.Height; 

            //bmp.Save("Note_" + _noteID.ToString() + ".png");
            BitmapImage image = Utilities.UtilitiesLib.convertBitmapToBitmapImage(bmp);
            contentDisplayer.Source = image;
        }
        
    }
}
