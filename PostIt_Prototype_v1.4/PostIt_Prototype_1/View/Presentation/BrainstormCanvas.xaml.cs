using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GenericIdeationObjects;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using PostIt_Prototype_1.NetworkCommunicator;
using PostIt_Prototype_1.PostItBrainstorming;
using PostIt_Prototype_1.PostItDataHandlers;
using PostIt_Prototype_1.PostItObjects;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Surface.Presentation.Generic;
using System.Windows.Media.Animation;
using System.Text;
using AppLimit.CloudComputing.SharpBox;
using MongoDB.Bson;
using PostIt_Prototype_1.Model.NetworkCommunicator;
using PostIt_Prototype_1.ModelView.PostItObjects;

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for BrainstormCanvas.xaml
    /// TODO: Remove Surface SDK dependency
    /// </summary>
    public partial class BrainstormCanvas : Window, IPointerManagerEventListener
    {
        #region Public Constructors

        // BackgroundWorker bgUploader = new BackgroundWorker();
        /// <summary>
        /// Default constructor.
        /// </summary>
        public BrainstormCanvas()
        {

            InitializeComponent();
            Loaded += new RoutedEventHandler(BrainstormCanvas_Loaded);
            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
            InitBrainstormingProcessors();
            InitNetworkCommManager();
            InitTimeline();

            DrawingCanvasModeSwitcher.NormalDrawingAttribute = DrawingCanvas.DefaultDrawingAttributes.Clone();

            var workingArea = System.Windows.Forms.Screen.AllScreens[Properties.Settings.Default.ActiveWorkingScreen].WorkingArea;
            this.Left = workingArea.Left;
            this.Top = workingArea.Top;
            this.Width = workingArea.Width;
            this.Height = workingArea.Height;
        }


        #endregion Public Constructors

        #region Private Methods

        private void AddMessageToListBox(string message)
        {
            /*
            this.Dispatcher.Invoke(new Action<string>((messageToAdd) =>
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = messageToAdd;
                lb_messages.Items.Add(item);
            }), new object[] { message });
            */
        }
        
        #region Note UI related
        
        private void AddNewIdeaUIs(List<IdeationUnit> ideas, bool asInit)
        {
            var rnd = new Random();

            Dispatcher.Invoke(new Action<List<IdeationUnit>, bool>((ideasToAdd, init) =>
            {
                foreach (var idea in ideasToAdd)
                {
                    if (!idea.IsAvailable)
                    {
                        continue;
                    }

                    if (idea is PostItNote)
                    {
                        AddSinglePostItNote(idea, rnd.Next(-40, 40), init);
                    }
                    else if (idea is StrokeBasedIdea)
                    {
                        AddSingleStrokeBasedNote(idea);
                    }
                }
                SvMainCanvas.UpdateLayout();
            }), new object[] { ideas, asInit });
        }

        private void AddSingleIdeaGroup(IdeationUnit ideaGroup)
        {
            try
            {
                var groupUi = new IdeaGroupUi();
                groupUi.SetNoteId(ideaGroup.Id);
                groupUi.Tag = ideaGroup;
                groupUi.Update(ideaGroup);
                var container = new ScatterViewItem();
                container.Content = groupUi;
                container.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                container.Width = groupUi.Width;
                container.Height = groupUi.Height;
                container.Orientation = 0;
                container.CanScale = false;
                container.CanRotate = false;
                container.Center = new System.Windows.Point(ideaGroup.CenterX, ideaGroup.CenterY);
                groupUi.InitContainer(container);
                SvMainCanvas.Items.Add(container);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        private void AddSinglePostItNote(IdeationUnit idea, int initAngle, bool init)
        {
            try
            {
                IPostItUi addedIdeaUi = null;
                var container = new ScatterViewItem();
                var castNote = (PostItNote)idea;
                if (castNote.Content is Bitmap)
                {
                    var noteUi = new ImageBasedPostItUi();

                    noteUi.Tag = idea;
                    noteUi.SetNoteId(castNote.Id);
                    noteUi.Update(idea);
                    noteUi.Width = noteUi.InitWidth;
                    noteUi.Height = noteUi.InitHeight;
                    if (castNote.MetaData.UiBackgroundColor.Length > 0)
                    {
                        noteUi.SetBackgroundPostItColor(castNote.MetaData.UiBackgroundColor);
                        noteUi.ApprovedNewBackgroundColor(castNote.MetaData.UiBackgroundColor);
                    }

                    container.Content = noteUi;
                    container.Width = noteUi.Width;
                    container.Height = noteUi.Height;

                    if (idea.CenterX == 0 && idea.CenterY == 0)
                    {
                        var centerX = (int)(container.Width / 2);
                        var centerY = (int)(SvMainCanvas.Height - container.Height / 2);
                        idea.CenterX = centerX;
                        idea.CenterY = centerY;
                    }
                    SvMainCanvas.Items.Add(container);
                    container.Center = new System.Windows.Point(idea.CenterX, idea.CenterY);
                    container.Orientation = initAngle;
                    container.ZIndex = 1;
                    container.CanScale = false;
                    addedIdeaUi = noteUi;
                    addedIdeaUi.InitContainer(container);
                }
                if (addedIdeaUi != null)
                {
                    if (init)
                    {
                        addedIdeaUi.StartJustAddedAnimation(container.Orientation);
                    }
                    addedIdeaUi.NoteUiTranslatedEventHandler += new NoteUiTranslatedEvent(noteUIManipluatedEventHandler);
                    addedIdeaUi.NoteUiDeletedEventHandler += new NoteUiDeletedEvent(noteUIDeletedEventHandler);
                    addedIdeaUi.NoteUiSizeChangedListener += new NoteUiSizeChangedEvent(addedIdeaUI_noteUISizeChangedListener);
                    addedIdeaUi.ColorPaletteLaunchedEventHandler += new ColorPaletteLaunchedEvent(addedIdeaUI_colorPaletteLaunchedEventHandler);
                }
                Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteAdded(idea));
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        void addedIdeaUI_colorPaletteLaunchedEventHandler(object sender, float posX, float posY)
        {
            var colorPalette = new PostItColorPalette();
            colorPalette.SetSize((sender as Control).Width, (sender as Control).Height);
            colorPalette.CallingControl = (Control)sender;
            colorPalette.ColorPickedEventHandler += new ColorPickedEvent(colorPalette_colorPickedEventHandler);
            colorPalette.SelectedColorApprovedHandler += new SelectedColorApproved(colorPalette_selectedColorApprovedHandler);
            MainGrid.Children.Add(colorPalette);
            var paletteMargin = colorPalette.Margin;
            paletteMargin.Left = posX - colorPalette.Width / 2;
            paletteMargin.Top = posY - colorPalette.Height / 2;
            colorPalette.Margin = paletteMargin;
            colorPalette.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            colorPalette.VerticalAlignment = System.Windows.VerticalAlignment.Top;
        }

        void colorPalette_colorPickedEventHandler(Control callingControl, string colorCode)
        {
            (callingControl as ImageBasedPostItUi).SetBackgroundPostItColor(colorCode);

        }

        void colorPalette_selectedColorApprovedHandler(object sender,Control callingControl,string approvedColorCode)
        {

            var palette = (sender as PostItColorPalette);
            var postItUi = (callingControl as ImageBasedPostItUi);
            if (postItUi.GetLatestApprovedtBackgroundColor().CompareTo(approvedColorCode) != 0)
            {
                postItUi.ApprovedNewBackgroundColor(approvedColorCode);
                TakeASnapshot();
                _brainstormManager.ChangeIdeaUiColor(postItUi.GetAssociatedIdea().Id, approvedColorCode);
            }
            MainGrid.Children.Remove(sender as PostItColorPalette);
        }
        
        private void AddSingleStrokeBasedNote(IdeationUnit strokeBasedIdea)
        {
            try
            {
                var ideaData = (StrokeData)(strokeBasedIdea.Content);
                var strokePoints = ideaData.StrokePoints;
                var stylusPoints = new StylusPointCollection();
                foreach (var p in strokePoints)
                {
                    var stylusP = new StylusPoint(p.X, p.Y);
                    stylusPoints.Add(stylusP);
                }
                var newStroke = new Stroke(stylusPoints);
                if (!ideaData.IsErasingStroke)
                {
                    newStroke.DrawingAttributes = DrawingCanvasModeSwitcher.NormalDrawingAttribute.Clone();
                    newStroke.DrawingAttributes.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(ideaData.StrokeColorCode);
                }
                else
                {
                    newStroke.DrawingAttributes = new DrawingAttributes();
                    newStroke.DrawingAttributes.Color = System.Windows.Media.Color.FromRgb(0, 0, 0);
                    newStroke.DrawingAttributes.Width = newStroke.DrawingAttributes.Height = 30;
                }
                DrawingCanvas.Strokes.Add(newStroke);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        private ScatterViewItem FindNoteContainerOfIdea(IdeationUnit idea)
        {
            ScatterViewItem matchedItem = null;
            foreach (ScatterViewItem item in SvMainCanvas.Items)
            {
                var noteUi = (IPostItUi)item.Content;
                if (noteUi.GetNoteId() == idea.Id)
                {
                    matchedItem = item;
                }
            }
            return matchedItem;
        }

        private void UpdateNoteUiContent(GenericIdeationObjects.IdeationUnit updatedIdea)
        {
            this.Dispatcher.Invoke(new Action<IdeationUnit>((ideaToUpdate) =>
            {
                try
                {
                    var noteContainer = FindNoteContainerOfIdea(ideaToUpdate);
                    var noteUi = (IPostItUi)noteContainer.Content;
                    noteUi.Update(ideaToUpdate);
                    noteContainer.Content = noteUi;
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }), new object[] { updatedIdea.Clone() });
        }

        private void UpdateNoteUiPosition(GenericIdeationObjects.IdeationUnit updatedIdea)
        {
            this.Dispatcher.Invoke(new Action<IdeationUnit>((ideaToUpdate) =>
            {
                try
                {
                    var noteContainer = FindNoteContainerOfIdea(ideaToUpdate);
                    if (noteContainer != null)
                    {
                        noteContainer.Center = new System.Windows.Point(ideaToUpdate.CenterX, ideaToUpdate.CenterY);
                        Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteMoved(ideaToUpdate));
                    }
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }), new object[] { updatedIdea });
        }

        private void RemoveNoteUi(IdeationUnit associatedIdea)
        {
            this.Dispatcher.Invoke(new Action<IdeationUnit>((ideaToDel) =>
            {
                try
                {
                    var ideaContainer = FindNoteContainerOfIdea(ideaToDel);
                    SvMainCanvas.Items.Remove(ideaContainer);
                    Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteDeleted(associatedIdea));
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }), new object[] { associatedIdea });
        }

        private void noteUIDeletedEventHandler(object sender, IdeationUnit associatedIdea)
        {
            _brainstormManager.RemoveIdea(associatedIdea);
        }

        void addedIdeaUI_noteUISizeChangedListener(object sender, IdeationUnit associatedIdea, float scaleX, float scaleY)
        {
            TakeASnapshot();
            Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteSizeChanged(associatedIdea, scaleX, scaleY));
        }

        private void noteUIManipluatedEventHandler(object sender, IdeationUnit underlyingIdea, float newX, float newY)
        {
            //auto adjust to align postit note vertically
            var container = FindNoteContainerOfIdea(underlyingIdea);
            if (Math.Abs(container.Orientation) <= 20)
            {
                container.Orientation = 0;
            }
            TakeASnapshot();
            _brainstormManager.UpdateIdeaPosition(underlyingIdea.Id, newX, newY);
        }

        private void noteUpdateScheduler_updateEventHandler()
        {
            //Thread dropboxImageNoteUpdateThread = new Thread(new ThreadStart(dropboxGeneralNoteDownloader.UpdateNotes));
            //dropboxImageNoteUpdateThread.Start();
            _dropboxGeneralNoteDownloader.UpdateNotes();

            //Thread anotoUpdateThread = new Thread(new ThreadStart(anotoNotesDownloader.UpdateNotes));
            //anotoUpdateThread.Start();
            _anotoNotesDownloader.UpdateNotes();
        }

        #endregion Note UI related
        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        private void BrainstormCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            SvMainCanvas.Width = this.ActualWidth;
            SvMainCanvas.Height = this.ActualHeight;
            DrawingCanvas.Width = this.ActualWidth;
            DrawingCanvas.Height = this.ActualHeight;
            CanvasesContainer.Width = this.ActualWidth;
            CanvasesContainer.Height = this.ActualHeight;
            MainGrid.Width = this.ActualWidth;
            MainGrid.Height = this.ActualHeight;

            LayoutMenu();

            Loaded -= BrainstormCanvas_Loaded;
        }

        private void brainstormManager_ideaAddedEventHandler(GenericIdeationObjects.IdeationUnit addedIdea)
        {
            // lock (sync)
            {
                var oneItemList = new List<IdeationUnit>();
                oneItemList.Add(addedIdea);
                AddNewIdeaUIs(oneItemList, true);
                TakeASnapshot();
                _timelineManager.AddAddChange(addedIdea);
            }
        }

        private void brainstormManager_ideaCollectionRollBackFinishedEventHandler(List<IdeationUnit> currentIdeas)
        {
            // lock (sync)
            {
                ClearNotes();
                AddNewIdeaUIs(currentIdeas, false);
                RecycleBin.RefreshNewDiscardedIdeasList(currentIdeas);
                Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_TimelineFrameFinishRetrieving());
                TakeASnapshot();
            }
        }

        private void brainstormManager_ideaRemovedHandler(GenericIdeationObjects.IdeationUnit removedIdea)
        {
            // lock (sync)
            {
                RemoveNoteUi(removedIdea);
                TakeASnapshot();
                _timelineManager.AddDeleteChange(removedIdea.Id);
            }
        }

        private void brainstormManager_ideaRestoredHandler(IdeationUnit restoredIdea)
        {
            // lock (sync)
            {
                var oneItemList = new List<IdeationUnit>();
                oneItemList.Add(restoredIdea);
                AddNewIdeaUIs(oneItemList, false);
                TakeASnapshot();
                _timelineManager.AddRestoreChange(restoredIdea.Id);
                Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteRestored(restoredIdea));
            }
        }

        private void brainstormManager_ideaUpdatedHandler(GenericIdeationObjects.IdeationUnit updatedIdea, GenericIdeationObjects.IdeationUnit.IdeaUpdateType updateType)
        {
            // lock (sync)
            {
                switch (updateType)
                {
                    case IdeationUnit.IdeaUpdateType.Position:
                        UpdateNoteUiPosition(updatedIdea);
                        break;

                    case IdeationUnit.IdeaUpdateType.Content:
                        UpdateNoteUiContent(updatedIdea);
                        break;
                }
                TakeASnapshot();
                switch (updateType)
                {
                    case IdeationUnit.IdeaUpdateType.Position:
                        _timelineManager.AddUpdatePositionChange(updatedIdea.Id, updatedIdea.CenterX, updatedIdea.CenterY);
                        break;

                    case IdeationUnit.IdeaUpdateType.Content:
                        _timelineManager.AddUpdateContentChange(updatedIdea.Id, updatedIdea.Content);
                        break;
                }
            }
        }

        void brainstormManager_ideaUIColorChangeHandler(IdeationUnit updatedIdea, string colorCode)
        {
            TakeASnapshot();
            _timelineManager.AddColorChange(updatedIdea.Id, colorCode);
        }

        private void ClearNotes()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    SvMainCanvas.Items.Clear();
                    SvMainCanvas.UpdateLayout();
                    DrawingCanvas.Strokes.Clear();
                    DrawingCanvas.UpdateLayout();
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }));
        }

        private void drawingCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            try
            {
                SvMainCanvas.IsHitTestVisible = true;
                var latestStroke = e.Stroke;
                var strokePoints = latestStroke.StylusPoints;
                if (!DrawingCanvasModeSwitcher.IsInErasingMode() && strokePoints.Count < 10)
                {
                    return;
                }
                var pathPoints = new List<System.Windows.Point>();
                foreach (var stylusP in strokePoints)
                {
                    var p = new System.Windows.Point();
                    p.X = stylusP.X;
                    p.Y = stylusP.Y;
                    pathPoints.Add(p);
                }
                /*if (Utilities.UtilitiesLib.CheckClosedPath(pathPoints))
                {
                    System.Windows.Point orginTopleft, orginBottomRight, orginCenter;
                    Utilities.UtilitiesLib.extractAnchorPointsOfPath(pathPoints,out orginTopleft,out orginBottomRight,out orginCenter);

                    IdeationUnitGroup idea = new IdeationUnitGroup();
                    IdeaGroupContentType ideaGroupContent = new IdeaGroupContentType();
                    ideaGroupContent.DisplayBoundaries = pathPoints;
                    idea.Content = ideaGroupContent;
                    idea.Id = IdeaIDGenerator.generateID();
                    idea.CenterX = (float)orginCenter.X;
                    idea.CenterY = (float)orginCenter.Y;

                    AddSingleIdeaGroup(idea);
                }*/
                //add corresponding idea object for this stroke
                IdeationUnit strokeIdea = new StrokeBasedIdea();
                strokeIdea.Id = IdeaIdGenerator.GenerateId();
                var strokeData = new StrokeData();
                strokeData.IsErasingStroke = DrawingCanvasModeSwitcher.IsInErasingMode();
                strokeData.StrokeColorCode = new System.Windows.Media.ColorConverter().ConvertToString(latestStroke.DrawingAttributes.Color);
                strokeData.StrokePoints = pathPoints;
                strokeIdea.Content = strokeData;
                _brainstormManager.AddIdeaInBackground(strokeIdea);
                //get the current screenshot
                TakeASnapshot();
                _timelineManager.AddAddChange(strokeIdea);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        private void InitBrainstormingProcessors()
        {
            _brainstormManager = new PostItGeneralManager();
            _brainstormManager.IdeaAddedEventHandler += new PostItGeneralManager.NewIdeaAddedEvent(brainstormManager_ideaAddedEventHandler);
            _brainstormManager.IdeaUpdatedHandler += new PostItGeneralManager.IdeaUpdatedEvent(brainstormManager_ideaUpdatedHandler);
            _brainstormManager.IdeaRemovedHandler += new PostItGeneralManager.IdeaRemovedEvent(brainstormManager_ideaRemovedHandler);
            _brainstormManager.IdeaRestoredHandler += new PostItGeneralManager.IdeaRestoredEvent(brainstormManager_ideaRestoredHandler);
            _brainstormManager.IdeaUiColorChangeHandler += new PostItGeneralManager.IdeaUiColorChangeEvent(brainstormManager_ideaUIColorChangeHandler);
            _brainstormManager.IdeaCollectionRollBackFinishedEventHandler += new PostItGeneralManager.IdeaCollectionRollBackFinished(brainstormManager_ideaCollectionRollBackFinishedEventHandler);

            _brainstormManager.TrashManager.DiscardedIdeaReceivedEventHandler += new Recycle_Bin.RecycleBinManager.DiscardedIdeaReceived(RecycleBin.AddDiscardedIdea);

            RecycleBin.NoteRestoredEventHandler += new Recycle_Bin.RecycleBinManager.DiscardedIdeaRestored(_brainstormManager.TrashManager.RestoreIdea);
        }

        private void InitNetworkCommManager()
        {
            //processors related to cloud service
            _dropboxGeneralNoteDownloader = new NoteUpdater();
            _anotoNotesDownloader = new AnotoNoteUpdater();
            _noteUpdateScheduler = new NoteUpdateScheduler();
            _noteUpdateScheduler.UpdateEventHandler += new NoteUpdateScheduler.UpdateIntervalTickedEvent(noteUpdateScheduler_updateEventHandler);
            _cloudDataEventProcessor = new CloudDataEventProcessor();
            _dropboxGeneralNoteDownloader.NoteStreamsDownloadedHandler += new NewNoteStreamsDownloaded(_cloudDataEventProcessor.HandleDownloadedStreamsFromCloud);
            _anotoNotesDownloader.NoteStreamsDownloadedHandler += new NewNoteStreamsDownloaded(_cloudDataEventProcessor.HandleDownloadedStreamsFromCloud);
            _cloudDataEventProcessor.NewNoteExtractedEventHandler += new CloudDataEventProcessor.NewNoteExtractedFromStreamEvent(_brainstormManager.HandleComingIdea);

            _p2PClient = new AsyncTcpClient();
            _remotePointerManager = new RemotePointerManager();
            _p2PClient.SetP2PDataListener(_remotePointerManager);
            _remotePointerManager.SetPointerEventListener(this);
            _p2PClient.StartClient();
        }

        private void InitTimeline()
        {
            TimelineView.FrameSelectedEventHandler += new BrainstormingTimelineUi.TimelineFrameSelected(timelineView_frameSelectedEventHandler);
            _timelineManager = new TimelineControllers.TimelineChangeManager();
            _timelineManager.NewTimelineFrameAddedEventHandler += new TimelineControllers.TimelineChangeManager.NewTimelineFrameAdded(newTimelineFrameAddedEventHandler);
            _timelineManager.StartEnumeratingEventHandler += new TimelineControllers.TimelineChangeManager.StartEnumeratingFromBeginning(_brainstormManager.Reset);
            _timelineManager.FinishEnumeratingEventHandler += new TimelineControllers.TimelineChangeManager.FinishEnumeratingToTheSelected(_brainstormManager.NotifyIdeaCollectionRollBack);

            var eventIntepreter = new TimelineControllers.TimlineEventIntepreter();
            eventIntepreter.AdDeventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.AddIdeaCommandExtracted(_brainstormManager.AddIdeaInBackground);
            eventIntepreter.RemovEeventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.RemoveIdeaCommandExtracted(_brainstormManager.RemoveIdeaInBackground);
            eventIntepreter.RestorEeventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.RestoreIdeaCommandExtraced(_brainstormManager.RestoreIdeaInBackground);
            eventIntepreter.UpdatePosEventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.UpdateIdeaPositionCommandExtracted(_brainstormManager.UpdateIdeaPositionInBackground);
            eventIntepreter.UpdateContentEventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.UpdateIdeaContentCommandExtracted(_brainstormManager.UpdateIdeaContentInBackground);
            eventIntepreter.ColorChangeEventExtractedHandler +=new TimelineControllers.TimlineEventIntepreter.ColorChangeCommandExtracted(_brainstormManager.ChangeIdeaUiColorInBackground);
            _timelineManager.EventIntepreter = eventIntepreter;
        }

        #region Menu related
        private void LayoutMenu()
        {
            //MainMenu.Radius = this.Height * 0.15;
            MainMenu.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            MainMenu.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            var mainMenuMargin = MainMenu.Margin;
            mainMenuMargin.Left = this.ActualWidth / 2 - MainMenu.Radius;
            mainMenuMargin.Top = this.ActualHeight - 2.25 * MainMenu.Radius;
            MainMenu.Margin = mainMenuMargin;
        }

        private void menuItem_DrawingSwitch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DrawingCanvasModeSwitcher.Flip();
                if (DrawingCanvasModeSwitcher.IsInErasingMode())
                {
                    DrawingCanvas.UsesTouchShape = false;
                    DrawingCanvas.DefaultDrawingAttributes.Color = System.Windows.Media.Color.FromRgb(0, 0, 0);
                    DrawingCanvas.DefaultDrawingAttributes.Width = DrawingCanvas.DefaultDrawingAttributes.Height = 30;
                    MenuItemDrawingSwitch.Header = MainWindow.Resources["PencilIcon"];
                }
                else
                {
                    DrawingCanvas.UsesTouchShape = true;
                    DrawingCanvas.DefaultDrawingAttributes = DrawingCanvasModeSwitcher.NormalDrawingAttribute.Clone();
                    MenuItemDrawingSwitch.Header = MainWindow.Resources["EraserIcon"];
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        private void menuItem_RecycleBin_Click(object sender, RoutedEventArgs e)
        {
            if (TimelineView.Visibility == System.Windows.Visibility.Visible)
            {
                TimelineView.FadeOut();
            }
            if (RecycleBin.Visibility == System.Windows.Visibility.Visible)
            {
                RecycleBin.FadeOut();
            }
            else
            {
                RecycleBin.FadeIn();
            }
        }

        private void menuItem_Timeline_Click(object sender, RoutedEventArgs e)
        {
            if (RecycleBin.Visibility == System.Windows.Visibility.Visible)
            {
                RecycleBin.FadeOut();
            }
            if (TimelineView.Visibility == System.Windows.Visibility.Hidden)
            {
                TimelineView.FadeIn();
            }
            else
            {
                TimelineView.FadeOut();
            }
        }

        private void newTimelineFrameAddedEventHandler(TimelineControllers.TimelineFrame addedFrame)
        {
            this.Dispatcher.Invoke(new Action<TimelineControllers.TimelineFrame>((frameToAdd) =>
            {
                TimelineView.AddFrame(frameToAdd);
            }), new TimelineControllers.TimelineFrame[] { addedFrame });
        }
        private void timelineView_frameSelectedEventHandler(int selectedFrameId)
        {
            Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_TimelineFrameStartRetrieving(selectedFrameId));
            _timelineManager.SelectFrame(selectedFrameId);
        }
        #endregion



        private void RefreshBrainstormingWhiteboard()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                SvMainCanvas.UpdateLayout();
            }));
        }


        private void sv_MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            SvMainCanvas.IsHitTestVisible = false;
        }

        private void sv_MainCanvas_TouchDown(object sender, TouchEventArgs e)
        {
            base.OnTouchDown(e);
            SvMainCanvas.IsHitTestVisible = false;
        }
        #region screenshot related
        private void TakeASnapshot()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    SvMainCanvas.UpdateLayout();
                    DrawingCanvas.UpdateLayout();
                    CanvasesContainer.UpdateLayout();
                    double dpi = 96;
                    //prepare to render the notes
                    var noteContainerRenderTargetBitmap = new RenderTargetBitmap((int)CanvasesContainer.ActualWidth, (int)CanvasesContainer.ActualHeight, dpi, dpi, PixelFormats.Pbgra32);
                    noteContainerRenderTargetBitmap.Render(CanvasesContainer);
                    var noteContainerImgSrc = (ImageSource)noteContainerRenderTargetBitmap.Clone();
                    var resizedNoteContainerBmpFrame = Utilities.UtilitiesLib.CreateResizedBitmapFrame(noteContainerImgSrc, (int)(CanvasesContainer.ActualWidth * 3 / 4), (int)(CanvasesContainer.Height * 3 / 4), 0);
                    var imageEncoder = new PngBitmapEncoder();
                    imageEncoder.Frames.Add(BitmapFrame.Create(resizedNoteContainerBmpFrame));
                    //imageEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                    var screenshotBytes = new byte[1];
                    using (var stream = new MemoryStream())
                    {
                        imageEncoder.Save(stream);
                        stream.Seek(0, 0);
                        screenshotBytes = stream.ToArray();
                        Utilities.GlobalObjects.CurrentScreenshotBytes = screenshotBytes;
                        //broadcastScreenshot(screenshotBytes);
                        Task.Factory.StartNew(() =>
                        {
                            BoardScreenUpdater.GetInstance(_dropboxGeneralNoteDownloader.Storage).UpdateMetaplanBoardScreen(new MemoryStream(screenshotBytes));
                        });
                        //bgUploader.RunWorkerAsync(new MemoryStream(screenshotBytes));
                    }

                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }));
        }
        public void BroadcastScreenshot(byte[] screenshotBytes)
        {
            var prefix = Encoding.UTF8.GetBytes("<WB_SCREEN>");
            var postfix = Encoding.UTF8.GetBytes("</WB_SCREEN>");
            var dataToSend = new byte[prefix.Length + screenshotBytes.Length + postfix.Length];
            var index = 0;
            Array.Copy(prefix, 0, dataToSend, index, prefix.Length);
            index += prefix.Length;
            Array.Copy(screenshotBytes, 0, dataToSend, index, screenshotBytes.Length);
            index += screenshotBytes.Length;
            Array.Copy(postfix, 0, dataToSend, index, postfix.Length);
            //p2pClient.Send(dataToSend);
            var header = AsyncTcpClient.CreatePacketHeader(dataToSend);
            _p2PClient.SyncSend(header);
            _p2PClient.SyncSend(dataToSend);
        }
        #endregion screenshot related


        #region Remote Pointer Related

        public void NewPointerAddedEvent(RemotePointer addedPointer, string assignedColorCode)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    var pointerContainer = new ScatterViewItem();
                    pointerContainer.ApplyTemplate();
                    var pointerUi = new RemotePointerUi();
                    pointerUi.PointerId = addedPointer.Id;
                    pointerUi.SetPointerColor(assignedColorCode);
                    pointerUi.Width = pointerUi.Height = 50;
                    pointerContainer.Width = pointerUi.Width;
                    pointerContainer.Height = pointerUi.Height;
                    //disable surrounding shadow
                    pointerContainer.Background = null;
                    pointerContainer.BorderThickness = new Thickness(0);
                    pointerContainer.ShowsActivationEffects = false;
                    //SurfaceShadowChrome ssc = pointerContainer.Template.FindName("shadow", pointerContainer) as SurfaceShadowChrome;
                    //ssc.Visibility = Visibility.Collapsed;
                    //add to the canvas and adjust location
                    pointerContainer.Content = pointerUi;
                    pointerContainer.Tag = addedPointer;
                    SvRemotePointerCanvas.Items.Add(pointerContainer);
                    var x = (int)(addedPointer.X * CanvasesContainer.Width);
                    var y = (int)(addedPointer.Y * CanvasesContainer.Height);
                    pointerContainer.Center = new System.Windows.Point(x, y);
                    SvRemotePointerCanvas.UpdateLayout();

                    Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_RemotePointerAdded(addedPointer));
                }
                catch (Exception ex)

                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }));
        }
        public void PointerUpdatedEvent(RemotePointer updatedPointer)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    var pointerContainer = FindViewItemWithPointerId(updatedPointer.Id);
                    if (pointerContainer == null)
                    {
                        return;
                    }
                    if (!updatedPointer.IsActive)
                    {
                        //make pointer fade out
                        var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(2));
                        pointerContainer.BeginAnimation(UserControl.OpacityProperty, anim);
                        //pointerContainer.Visibility = System.Windows.Visibility.Hidden;
                        SvRemotePointerCanvas.UpdateLayout();
                        pointerContainer.Tag = updatedPointer;
                        Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_RemotePointerLeft(updatedPointer));
                    }
                    else
                    {
                        //make pointer fade in
                        if (!((RemotePointer)pointerContainer.Tag).IsActive)
                        {
                            var anim = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2));
                            pointerContainer.BeginAnimation(UserControl.OpacityProperty, anim);
                            Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_RemotePointerReentered(updatedPointer));
                        }
                        //update location
                        var x = (int)(updatedPointer.X * CanvasesContainer.Width);
                        var y = (int)(updatedPointer.Y * CanvasesContainer.Height);
                        pointerContainer.Center = new System.Windows.Point(x, y);
                        if (((RemotePointer)pointerContainer.Tag).IsActive)
                        {
                            Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_RemotePointerMoved(updatedPointer));
                        }
                        pointerContainer.Tag = updatedPointer;
                        SvRemotePointerCanvas.UpdateLayout();
                    }
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }));
        }
        ScatterViewItem FindViewItemWithPointerId(int pointerId)
        {
            ScatterViewItem matchedItem = null;
            foreach (ScatterViewItem item in SvRemotePointerCanvas.Items)
            {
                var remotePointer = (RemotePointerUi)(item.Content);
                if (remotePointer.PointerId == pointerId)
                {
                    matchedItem = item;
                    break;
                }
            }
            return matchedItem;
        }
        #endregion Remote Pointer Related
        #endregion Private Methods

        #region Private Fields

        private static object _sync = new object();
        private AnotoNoteUpdater _anotoNotesDownloader = null;
        private PostItGeneralManager _brainstormManager;
        private CloudDataEventProcessor _cloudDataEventProcessor = null;

        private NoteUpdater  _dropboxGeneralNoteDownloader = null;


        private AsyncTcpClient _p2PClient = null;
        private RemotePointerManager _remotePointerManager = null;
        //Network data processors
        private NoteUpdateScheduler _noteUpdateScheduler = null;

        //PostItNetworkDataManager networkDataManager = null;
        //Timeline processors
        private TimelineControllers.TimelineChangeManager _timelineManager;


        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LayoutMenu();
        }
        #endregion Private Fields

        #region window system events
        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }
        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Maximized;
            this.WindowStyle = System.Windows.WindowStyle.None;
            Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_Start());
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Utilities.BrainstormingEventLogger.GetInstance(_dropboxGeneralNoteDownloader.Storage).Close();
        }
        /// <summary>
        /// Occurs when the window is about to close.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
            try
            {
                _noteUpdateScheduler.Stop();
                _dropboxGeneralNoteDownloader.Close();
                _anotoNotesDownloader.Close();

            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        #endregion window system events
        /* Runs on UI thread */
    }
}