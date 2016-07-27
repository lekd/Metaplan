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
using PostIt_Prototype_1.ModelView.PostItObjects;

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for ImageBasedPostItUI.xaml
    /// </summary>
    public partial class ImageBasedPostItUi : UserControl, IPostItUi
    {


        int _noteId;
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
        public int GetNoteId()
        {
            return _noteId;
        }
        public void SetNoteId(int id)
        {
            _noteId = id;
        }
        public GenericIdeationObjects.IdeationUnit GetAssociatedIdea()
        {
            return (GenericIdeationObjects.IdeationUnit)this.Tag;
        }
        public event NoteUiTranslatedEvent NoteUiTranslatedEventHandler = null;
        public event NoteUiDeletedEvent NoteUiDeletedEventHandler = null;
        public event NoteUiSizeChangedEvent NoteUiSizeChangedListener = null;
        public event ColorPaletteLaunchedEvent ColorPaletteLaunchedEventHandler = null;
        Control _container;
        string _backgroundColor;
        public Control Container
        {
            get { return _container; }
            set { _container = value; }
        }
        public ImageBasedPostItUi()
        {
            InitializeComponent();
            _backgroundColor = "#ffffaa";
        }
        public string GetLatestApprovedtBackgroundColor()
        {
            return _backgroundColor;
        }
        public void SetBackgroundPostItColor(string colorCode)
        {
            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString(colorCode);
            if (convertFromString != null)
                this.Background = new SolidColorBrush((System.Windows.Media.Color)convertFromString);
            this.UpdateLayout();
        }

        public void ApprovedNewBackgroundColor(string colorCode)
        {
            _backgroundColor = colorCode;
        }
        public void Update(GenericIdeationObjects.IdeationUnit idea)
        {
            var bmpContent = PostItNote.DeepClone(idea.Content) as Bitmap;
            UpdateDisplayedContent(bmpContent);
            this.Tag = idea;
        }
        System.Windows.Point _quickZoomSize = new System.Windows.Point();
        public void UpdateDisplayedContent(object content)
        {
            var bmp = (Bitmap)content;
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
                _quickZoomSize.X = _initWidth * Properties.Settings.Default.ZoomInScale;
                _quickZoomSize.Y = _initHeight * Properties.Settings.Default.ZoomInScale;
                //bmp.Save("Note_" + _noteID.ToString() + ".png");
                var image = Utilities.UtilitiesLib.ConvertBitmapToBitmapImage(bmp);
                ContentDisplayer.Source = image;
                this.UpdateLayout();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        Storyboard _ringingAnimationSb = new Storyboard();
        public void InitContainer(Control container)
        {
            _container = container;
            _container.IsManipulationEnabled = true;
            _container.AddHandler(UIElement.ManipulationStartedEvent, new EventHandler<ManipulationStartedEventArgs>(container_ManipulationStarted), true);
            _container.AddHandler(UIElement.ManipulationCompletedEvent, new EventHandler<ManipulationCompletedEventArgs>(container_ManipulationCompleted), true);
        }
        public void StartJustAddedAnimation(double initRotation)
        {
            var rnd = new Random();
            var rotAnimation = new DoubleAnimation(initRotation - 10, initRotation + 10, new System.Windows.Duration(TimeSpan.FromSeconds(0.4)));
            rotAnimation.AutoReverse = true;
            rotAnimation.RepeatBehavior = RepeatBehavior.Forever;
            _ringingAnimationSb.Children.Add(rotAnimation);
            Storyboard.SetTarget(rotAnimation, _container);
            Storyboard.SetTargetProperty(rotAnimation, new PropertyPath(ScatterViewItem.OrientationProperty));
            _ringingAnimationSb.Begin();

        }

        void container_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var associatedIdea = (GenericIdeationObjects.IdeationUnit)this.Tag;
            var container = (ScatterViewItem)sender;
            if (NoteUiTranslatedEventHandler != null)
            {
                NoteUiTranslatedEventHandler(this, (GenericIdeationObjects.IdeationUnit)this.Tag, (float)container.Center.X, (float)container.Center.Y);
            }
        }

        void container_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _ringingAnimationSb.Stop();
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            if (NoteUiDeletedEventHandler != null)
            {
                NoteUiDeletedEventHandler(this, (GenericIdeationObjects.IdeationUnit)this.Tag);
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
                if (NoteUiTranslatedEventHandler != null)
                {
                    NoteUiTranslatedEventHandler(this, (GenericIdeationObjects.IdeationUnit)this.Tag, (float)(_container as ScatterViewItem).Center.X, (float)(_container as ScatterViewItem).Center.Y);
                }
            }
        }

        private void btn_Zoomout_Click(object sender, RoutedEventArgs e)
        {
            BtnZoomout.Visibility = Visibility.Hidden;
            BtnZoomIn.Visibility = Visibility.Visible;
            var prevW = this.Width;
            var prevH = this.Height;
            this.Width = _initWidth;
            this.Height = _initHeight;
            _container.Width = this.Width;
            _container.Height = this.Height;
            if (NoteUiSizeChangedListener != null)
            {
                NoteUiSizeChangedListener(this, GetAssociatedIdea(), (float)(this.Width / prevW), (float)(this.Height / prevH));
            }
        }

        private void btn_ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            BtnZoomIn.Visibility = Visibility.Hidden;
            BtnZoomout.Visibility = Visibility.Visible;
            var prevW = this.Width;
            var prevH = this.Height;
            this.Width = _quickZoomSize.X;
            this.Height = _quickZoomSize.Y;
            _container.Width = this.Width;
            _container.Height = this.Height;
            if (NoteUiSizeChangedListener != null)
            {
                NoteUiSizeChangedListener(this, GetAssociatedIdea(), (float)(this.Width / prevW), (float)(this.Height / prevH));
            }
        }
        public void DisableZoomButtons()
        {
            BtnZoomIn.Visibility = Visibility.Hidden;
            BtnZoomout.Visibility = Visibility.Hidden;
        }

        private void btn_ColorPicker_Click(object sender, RoutedEventArgs e)
        {
            if (ColorPaletteLaunchedEventHandler != null)
            {
                ColorPaletteLaunchedEventHandler(this, GetAssociatedIdea().CenterX, GetAssociatedIdea().CenterY);
            }
        }

    }
}
