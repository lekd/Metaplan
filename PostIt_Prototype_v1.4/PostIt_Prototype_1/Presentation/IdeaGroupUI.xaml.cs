using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for IdeaGroupUI.xaml
    /// </summary>
    public partial class IdeaGroupUI : UserControl, IPostItUI
    {
        #region Public Constructors

        public IdeaGroupUI()
        {
            InitializeComponent();
            GroupShape.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 255, 0));
        }

        #endregion Public Constructors

        #region Public Methods

        public GenericIdeationObjects.IdeationUnit getAssociatedIdea()
        {
            return (GenericIdeationObjects.IdeationUnit)this.Tag;
        }

        public int getNoteID()
        {
            return _noteID;
        }

        public void InitContainer(Control container)
        {
            _container = container;
            _container.Width = this.Width;
            _container.Height = this.Height;
            _container.IsManipulationEnabled = true;
        }

        public void setNoteID(int id)
        {
            _noteID = id;
        }

        public void startJustAddedAnimation(double initRotation)
        {
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
            Utilities.UtilitiesLib.extractAnchorPointsOfPath(boundaryPoints, out topleft, out bottomright, out center);
            this.Width = bottomright.X - topleft.X;
            this.Height = bottomright.Y - topleft.Y;
        }

        #endregion Public Methods

        #region Public Events

        public event ColorPaletteLaunchedEvent colorPaletteLaunchedEventHandler;

        public event NoteUIDeletedEvent noteUIDeletedEventHandler;

        public event NoteUISizeChangedEvent noteUISizeChangedListener;

        public event NoteUITranslatedEvent noteUITranslatedEventHandler;

        #endregion Public Events

        #region Public Properties

        public Control Container
        {
            get { return _container; }
            set { _container = value; }
        }

        #endregion Public Properties

        #region Private Fields

        private Control _container;
        private int _noteID;

        #endregion Private Fields
    }
}