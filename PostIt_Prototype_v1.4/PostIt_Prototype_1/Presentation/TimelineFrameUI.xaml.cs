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

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for TimelineFrameUI.xaml
    /// </summary>
    public partial class TimelineFrameUI : UserControl
    {
        bool isMouseDown = false;
        public delegate void FrameSelectedEvent(object sender);
        public event FrameSelectedEvent onFrameSelected = null;
        public TimelineFrameUI()
        {
            InitializeComponent();
        }
        public void setFrameContent(Bitmap frameBmp)
        {
            FrameContent.Source = Utilities.UtilitiesLib.convertBitmapToBitmapImage(frameBmp);
        }
        public void setSelected(bool isSelected)
        {
            if (isSelected)
            {
                FrameBorder.BorderBrush = System.Windows.Media.Brushes.Cyan;
            }
            else
            {
                FrameBorder.BorderBrush = System.Windows.Media.Brushes.DarkGray;
            }
            this.UpdateLayout();
        }

        private void FrameContent_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
        }
        private void FrameContent_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMouseDown)
            {
                if (onFrameSelected != null)
                {
                    onFrameSelected(this);
                }
            }
        }

        private void FrameContent_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (onFrameSelected != null)
            {
                onFrameSelected(this);
            }
        }

        private void FrameContent_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            if (onFrameSelected != null)
            {
                onFrameSelected(this);
            }
        }

        private void FrameContent_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isMouseDown)
            {
                if (onFrameSelected != null)
                {
                    onFrameSelected(this);
                }
            }
        }
        public byte[] getCurrentDisplayBitmapBytes()
        {
            try
            {
                return Utilities.UtilitiesLib.BitmapImageToBytes((BitmapImage)FrameContent.Source);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(
                    "TimelineFrameUI-getCurrentDisplayBitmapBytes: ", ex);
                return null;
            }
            
        }
    }
}
