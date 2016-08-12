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
using System.IO;
using System.Windows.Media.Animation;

namespace WhiteboardApp.Presentation
{
    /// <summary>
    /// Interaction logic for BrainstormingTimelineUI.xaml
    /// </summary>
    public partial class BrainstormingTimelineUI : UserControl
    {
        public delegate void TimelineFrameSelected(int selectedFrameID);
        TimelineFrameUI currentSelectedFrame = null;
        public event TimelineFrameSelected frameSelectedEventHandler = null;
        public BrainstormingTimelineUI()
        {
            InitializeComponent();
        }

        public void AddFrame(TimelineControllers.TimelineFrame frame)
        {
            try
            {
                Bitmap originScreenshot = null;
                if (frame.Change.ChangeType == TimelineControllers.TypeOfChange.DUPLICATE)
                {
                    using (var memStream = new MemoryStream(Utilities.GlobalObjects.lastRollBackScreenshotBytes))
                    {
                        originScreenshot = new Bitmap(memStream);
                    }
                }
                else
                {
                    if (Utilities.GlobalObjects.currentScreenshotBytes != null)
                        using (var memStream = new MemoryStream(Utilities.GlobalObjects.currentScreenshotBytes))
                        {
                            originScreenshot = new Bitmap(memStream);
                        }
                }

                if (originScreenshot == null)
                    return;

                originScreenshot = scaleScreenshot(originScreenshot);
                var frameUI = new TimelineFrameUI();
                frameUI.setFrameContent(originScreenshot);
                frameUI.Tag = frame;
                screenshotContainer.Children.Add(frameUI);
                frameUI.onFrameSelected += new TimelineFrameUI.FrameSelectedEvent(frameUI_onFrameSelected);
                moveToFrame(frameUI);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }

        }
        void moveToFrame(TimelineFrameUI targetFrame)
        {
            try
            {
                if (currentSelectedFrame != null)
                {
                    currentSelectedFrame.setSelected(false);
                }
                currentSelectedFrame = targetFrame;
                currentSelectedFrame.setSelected(true);
                screenshotContainer.ScrollOwner.ScrollToRightEnd();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        void frameUI_onFrameSelected(object sender)
        {
            try
            {
                if (currentSelectedFrame != null)
                {
                    currentSelectedFrame.setSelected(false);
                }
                var selectedFrame = (TimelineFrameUI)sender;
                Utilities.GlobalObjects.lastRollBackScreenshotBytes = selectedFrame.getCurrentDisplayBitmapBytes();
                selectedFrame.setSelected(true);
                currentSelectedFrame = selectedFrame;

                var frameData = (TimelineControllers.TimelineFrame)selectedFrame.Tag;
                if (frameSelectedEventHandler != null)
                {
                    frameSelectedEventHandler(frameData.Id);
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        public void FadeIn()
        {
            var anim = new DoubleAnimation(1, TimeSpan.FromSeconds(1));
            this.BeginAnimation(UserControl.OpacityProperty, anim);
            this.Visibility = System.Windows.Visibility.Visible;
        }
        public void FadeOut()
        {
            var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
            this.BeginAnimation(UserControl.OpacityProperty, anim);
            this.Visibility = System.Windows.Visibility.Hidden;
        }
        Bitmap scaleScreenshot(Bitmap originScreenshot)
        {
            try
            {
                var height = (int)(this.Height * 2 / 3);
                var width = (int)(originScreenshot.Width * height / originScreenshot.Height);
                var scaled = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (var gr = Graphics.FromImage(scaled))
                {
                    gr.DrawImage(originScreenshot, new System.Drawing.Rectangle(0, 0, width, height));
                }
                return scaled;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
                return originScreenshot;
            }
        }
    }
}
