﻿using System;
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

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for BrainstormingTimelineUI.xaml
    /// </summary>
    public partial class BrainstormingTimelineUi : UserControl
    {
        public delegate void TimelineFrameSelected(int selectedFrameId);
        TimelineFrameUi _currentSelectedFrame = null;
        public event TimelineFrameSelected FrameSelectedEventHandler = null;
        public BrainstormingTimelineUi()
        {
            InitializeComponent();
        }
        
        public void AddFrame(TimelineControllers.TimelineFrame frame)
        {
            try
            {
                Bitmap originScreenshot = null;
                if (frame.Change.ChangeType == TimelineControllers.TypeOfChange.Duplicate)
                {
                    using (var memStream = new MemoryStream(Utilities.GlobalObjects.LastRollBackScreenshotBytes))
                    {
                        originScreenshot = new Bitmap(memStream);
                    }
                }
                else
                {
                    using (var memStream = new MemoryStream(Utilities.GlobalObjects.CurrentScreenshotBytes))
                    {
                        originScreenshot = new Bitmap(memStream);
                    }
                }
                if (originScreenshot != null)
                {
                    originScreenshot = ScaleScreenshot(originScreenshot);
                    var frameUi = new TimelineFrameUi();
                    frameUi.SetFrameContent(originScreenshot);
                    frameUi.Tag = frame;
                    ScreenshotContainer.Children.Add(frameUi);
                    frameUi.OnFrameSelected += new TimelineFrameUi.FrameSelectedEvent(frameUI_onFrameSelected);
                    MoveToFrame(frameUi);
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            
        }
        void MoveToFrame(TimelineFrameUi targetFrame)
        {
            try
            {
                if (_currentSelectedFrame != null)
                {
                    _currentSelectedFrame.SetSelected(false);
                }
                _currentSelectedFrame = targetFrame;
                _currentSelectedFrame.SetSelected(true);
                ScreenshotContainer.ScrollOwner.ScrollToRightEnd();
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
                if (_currentSelectedFrame != null)
                {
                    _currentSelectedFrame.SetSelected(false);
                }
                var selectedFrame = (TimelineFrameUi)sender;
                Utilities.GlobalObjects.LastRollBackScreenshotBytes = selectedFrame.GetCurrentDisplayBitmapBytes();
                selectedFrame.SetSelected(true);
                _currentSelectedFrame = selectedFrame;

                var frameData = (TimelineControllers.TimelineFrame)selectedFrame.Tag;
                if (FrameSelectedEventHandler != null)
                {
                    FrameSelectedEventHandler(frameData.Id);
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
            this.BeginAnimation(UserControl.OpacityProperty,anim);
            this.Visibility = System.Windows.Visibility.Visible;
        }
        public void FadeOut()
        {
            var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
            this.BeginAnimation(UserControl.OpacityProperty, anim);
            this.Visibility = System.Windows.Visibility.Hidden;
        }
        Bitmap ScaleScreenshot(Bitmap originScreenshot)
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