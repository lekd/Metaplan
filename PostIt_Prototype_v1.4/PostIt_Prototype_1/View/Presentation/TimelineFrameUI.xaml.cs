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
    public partial class TimelineFrameUi : UserControl
    {
        bool _isMouseDown = false;
        public delegate void FrameSelectedEvent(object sender);
        public event FrameSelectedEvent OnFrameSelected = null;
        public TimelineFrameUi()
        {
            InitializeComponent();
        }
        public void SetFrameContent(Bitmap frameBmp)
        {
            FrameContent.Source = Utilities.UtilitiesLib.ConvertBitmapToBitmapImage(frameBmp);
        }
        public void SetSelected(bool isSelected)
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
            _isMouseDown = true;
        }
        private void FrameContent_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMouseDown)
            {
                if (OnFrameSelected != null)
                {
                    OnFrameSelected(this);
                }
            }
        }

        private void FrameContent_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (OnFrameSelected != null)
            {
                OnFrameSelected(this);
            }
        }

        private void FrameContent_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            if (OnFrameSelected != null)
            {
                OnFrameSelected(this);
            }
        }

        private void FrameContent_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMouseDown)
            {
                if (OnFrameSelected != null)
                {
                    OnFrameSelected(this);
                }
            }
        }
        public byte[] GetCurrentDisplayBitmapBytes()
        {
            try
            {
                return Utilities.UtilitiesLib.BitmapImageToBytes((BitmapImage)FrameContent.Source);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
                return null;
            }
            
        }
    }
}
