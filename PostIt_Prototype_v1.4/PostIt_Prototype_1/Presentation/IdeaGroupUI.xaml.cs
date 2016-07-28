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
    /// Interaction logic for IdeaGroupUI.xaml
    /// </summary>
    public partial class IdeaGroupUI : UserControl, IPostItUI
    {
        public event NoteUITranslatedEvent noteUITranslatedEventHandler = null;
        public event NoteUIDeletedEvent noteUIDeletedEventHandler = null;
        public event NoteUISizeChangedEvent noteUISizeChangedListener = null;
        public event ColorPaletteLaunchedEvent colorPaletteLaunchedEventHandler = null;
        public IdeaGroupUI()
        {
            InitializeComponent();
            GroupShape.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 255, 0));
        }

        public GenericIdeationObjects.IdeationUnit getAssociatedIdea()
        {
            return (GenericIdeationObjects.IdeationUnit)this.Tag;
        }
        Control _container;
        public Control Container
        {
            get { return _container; }
            set { _container = value; }
        }
        int _noteID;
        public int getNoteID()
        {
            return _noteID;
        }
        public void setNoteID(int id)
        {
            _noteID = id;
        }
        public void update(GenericIdeationObjects.IdeationUnit idea)
        {
            if (idea is GenericIdeationObjects.IdeationUnitGroup)
            {
                var groupContent = (GenericIdeationObjects.IdeaGroupContentType)idea.Content;
                updateDisplayedContent(groupContent.DisplayBoundaries);
            }
        }
        public void updateDisplayedContent(object content)
        {
            var boundaryPoints = ((List<Point>)content);
            var shiftedPath = Utilities.UtilitiesLib.shiftPathToCoordinateOrigin(boundaryPoints);
            var polygonPoints = new PointCollection(shiftedPath);
            GroupShape.Points = polygonPoints;
            Point topleft, bottomright, center;
            Utilities.UtilitiesLib.extractAnchorPointsOfPath(boundaryPoints, out topleft, out bottomright,out center);
            this.Width = bottomright.X - topleft.X;
            this.Height = bottomright.Y - topleft.Y;
        }
        public void InitContainer(Control container)
        {
            _container = container;
            _container.Width = this.Width;
            _container.Height = this.Height;
            _container.IsManipulationEnabled = true;
        }
        public void startJustAddedAnimation(double initRotation)
        {

        }
    }
}
