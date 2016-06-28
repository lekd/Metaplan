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
using System.Windows.Media.Animation;
using Microsoft.Surface.Presentation.Controls;

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
        public GenericIdeationObjects.IdeationUnit getAssociatedIdea()
        {
            return (GenericIdeationObjects.IdeationUnit)this.Tag;
        }
        public event NoteUITranslatedEvent noteUITranslatedEventHandler = null;
        public event NoteUIDeletedEvent noteUIDeletedEventHandler = null;
        public event NoteUISizeChangedEvent noteUISizeChangedListener = null;
        Control _container;

        public Control Container
        {
            get { return _container; }
            set { _container = value; }
        }
        public ImageBasedPostItUI()
        {
            InitializeComponent();
        }
        public void setBackgroundPostItColor(string colorCode)
        {
            this.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorCode));
            this.UpdateLayout();
        }
        public void update(GenericIdeationObjects.IdeationUnit idea)
        {
            Bitmap bmpContent = PostItNote.DeepClone(idea.Content) as Bitmap;
            updateDisplayedContent(bmpContent);
            this.Tag = idea;
        }
        System.Windows.Point quickZoomSize = new System.Windows.Point();
        public void updateDisplayedContent(object content)
        {
            Bitmap bmp = (Bitmap)content;
            if (content == null)
            {
                return;
            }
            try
            {
                _initWidth = bmp.Width;
                _initHeight = bmp.Height;
                if (bmp.Width == bmp.Height)
                {
                    _initWidth = Properties.Settings.Default.PostItNoteWidth;
                    _initHeight = Properties.Settings.Default.PostItNoteHeight;
                }
                quickZoomSize.X = _initWidth * Properties.Settings.Default.ZoomInScale;
                quickZoomSize.Y = _initHeight * Properties.Settings.Default.ZoomInScale;
                //bmp.Save("Note_" + _noteID.ToString() + ".png");
                BitmapImage image = Utilities.UtilitiesLib.convertBitmapToBitmapImage(bmp);
                contentDisplayer.Source = image;
                this.UpdateLayout();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        Storyboard ringingAnimationSB = new Storyboard();
        public void InitContainer(Control container)
        {
            _container = container;
            _container.IsManipulationEnabled = true;
            _container.AddHandler(UIElement.ManipulationStartedEvent, new EventHandler<ManipulationStartedEventArgs>(container_ManipulationStarted), true);
            _container.AddHandler(UIElement.ManipulationCompletedEvent, new EventHandler<ManipulationCompletedEventArgs>(container_ManipulationCompleted), true);
        }
        public void startJustAddedAnimation(double initRotation)
        {
            Random rnd = new Random();
            var rotAnimation = new DoubleAnimation(initRotation - 10, initRotation + 10, new System.Windows.Duration(TimeSpan.FromSeconds(0.4)));
            rotAnimation.AutoReverse = true;
            rotAnimation.RepeatBehavior = RepeatBehavior.Forever;
            ringingAnimationSB.Children.Add(rotAnimation);
            Storyboard.SetTarget(rotAnimation, _container);
            Storyboard.SetTargetProperty(rotAnimation, new PropertyPath(ScatterViewItem.OrientationProperty));
            ringingAnimationSB.Begin();

        }

        void container_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            GenericIdeationObjects.IdeationUnit associatedIdea = (GenericIdeationObjects.IdeationUnit)this.Tag;
            ScatterViewItem container = (ScatterViewItem)sender;
            if (noteUITranslatedEventHandler != null)
            {
                noteUITranslatedEventHandler(this, (GenericIdeationObjects.IdeationUnit)this.Tag, (float)container.Center.X, (float)container.Center.Y);
            }
        }

        void container_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ringingAnimationSB.Stop();
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            if (noteUIDeletedEventHandler != null)
            {
                noteUIDeletedEventHandler(this, (GenericIdeationObjects.IdeationUnit)this.Tag);
            }
        }

        private void PostInMainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_container != null)
            {
                /*if (_container.Width != _initWidth
                    || _container.Height != _initHeight)
                {
                    _container.Width = _initWidth;
                    _container.Height = _initHeight;
                }
                else
                {
                    _container.Width = quickZoomSize.X;
                    _container.Height = quickZoomSize.Y;
                }*/
                (_container as ScatterViewItem).Orientation = 0;
            }
        }

        private void btn_Zoomout_Click(object sender, RoutedEventArgs e)
        {
            btn_Zoomout.Visibility = Visibility.Hidden;
            btn_ZoomIn.Visibility = Visibility.Visible;
            double prevW = this.Width;
            double prevH = this.Height;
            this.Width = _initWidth;
            this.Height = _initHeight;
            _container.Width = this.Width;
            _container.Height = this.Height;
            if (noteUISizeChangedListener != null)
            {
                noteUISizeChangedListener(this, getAssociatedIdea(), (float)(this.Width / prevW), (float)(this.Height / prevH));
            }
        }

        private void btn_ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            btn_ZoomIn.Visibility = Visibility.Hidden;
            btn_Zoomout.Visibility = Visibility.Visible;
            double prevW = this.Width;
            double prevH = this.Height;
            this.Width = quickZoomSize.X;
            this.Height = quickZoomSize.Y;
            _container.Width = this.Width;
            _container.Height = this.Height;
            if (noteUISizeChangedListener != null)
            {
                noteUISizeChangedListener(this, getAssociatedIdea(), (float)(this.Width / prevW), (float)(this.Height / prevH));
            }
        }
        public void DisableZoomButtons()
        {
            btn_ZoomIn.Visibility = Visibility.Hidden;
            btn_Zoomout.Visibility = Visibility.Hidden;
        }

        private void btn_ColorPicker_Click(object sender, RoutedEventArgs e)
        {
            PostItColorPalette colorPalette = new PostItColorPalette();
            Grid.SetRow(colorPalette, 0);
            Grid.SetRowSpan(colorPalette, 3);
            Grid.SetColumn(colorPalette, 0);
            Grid.SetColumnSpan(colorPalette, 3);
            MainGrid.Children.Add(colorPalette);
            colorPalette.colorPickedEventHandler += new ColorPickedEvent(colorPalette_colorPickedEventHandler);
        }

        void colorPalette_colorPickedEventHandler(Control callingControl, string colorCode)
        {
            setBackgroundPostItColor(colorCode);
        }

    }
}
