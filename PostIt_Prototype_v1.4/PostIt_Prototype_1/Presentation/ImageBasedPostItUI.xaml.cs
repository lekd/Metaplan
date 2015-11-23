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
                quickZoomSize.X = _initWidth * 1.5;
                quickZoomSize.Y = _initHeight * 1.5;
                //bmp.Save("Note_" + _noteID.ToString() + ".png");
                BitmapImage image = Utilities.UtilitiesLib.convertBitmapToBitmapImage(bmp);
                contentDisplayer.Source = image;
                this.UpdateLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show("updateDisplayedContent: " + ex.StackTrace);
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
                if (_container.Width != _initWidth
                    || _container.Height != _initHeight)
                {
                    _container.Width = _initWidth;
                    _container.Height = _initHeight;
                }
                else
                {
                    _container.Width = quickZoomSize.X;
                    _container.Height = quickZoomSize.Y;
                }
            }
        }


    }
}
