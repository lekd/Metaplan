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
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Surface.Presentation.Generic;
using System.Windows.Media.Animation;
using System.Text;
using PostIt_Prototype_1.Utilities;

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for BrainstormCanvas.xaml
    /// TODO: Remove Surface SDK dependency
    /// </summary>
    public partial class BrainstormCanvas : Window, PointerManagerEventListener
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

            Dispatcher.Invoke(new Action<List<IdeationUnit>, bool>(async (ideasToAdd, init) =>
            {
                foreach (var idea in ideasToAdd)
                {
                    if (!idea.IsAvailable)
                    {
                        continue;
                    }

                    if (idea is PostItNote)
                    {
                        await AddSinglePostItNote(idea, rnd.Next(-40, 40), init);
                    }
                    else if (idea is StrokeBasedIdea)
                    {
                        AddSingleStrokeBasedNote(idea);
                    }
                }
                sv_MainCanvas.UpdateLayout();
            }), new object[] { ideas, asInit });
        }

        private void AddSingleIdeaGroup(IdeationUnit ideaGroup)
        {
            try
            {
                var groupUi = new IdeaGroupUI();
                groupUi.setNoteID(ideaGroup.Id);
                groupUi.Tag = ideaGroup;
                groupUi.update(ideaGroup);
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
                sv_MainCanvas.Items.Add(container);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        private async Task AddSinglePostItNote(IdeationUnit idea, int initAngle, bool init)
        {
            try
            {
                IPostItUI addedIdeaUi = null;
                var container = new ScatterViewItem();
                var castNote = (PostItNote)idea;
                if (castNote.Content is Bitmap)
                {
                    var noteUi = new ImageBasedPostItUI();

                    noteUi.Tag = idea;
                    noteUi.setNoteID(castNote.Id);
                    noteUi.update(idea);
                    noteUi.Width = noteUi.InitWidth;
                    noteUi.Height = noteUi.InitHeight;
                    if (castNote.MetaData.UiBackgroundColor.Length > 0)
                    {
                        noteUi.setBackgroundPostItColor(castNote.MetaData.UiBackgroundColor);
                        noteUi.approvedNewBackgroundColor(castNote.MetaData.UiBackgroundColor);
                    }

                    container.Content = noteUi;
                    container.Width = noteUi.Width;
                    container.Height = noteUi.Height;

                    if (idea.CenterX == 0 && idea.CenterY == 0)
                    {
                        var centerX = (int)(container.Width / 2);
                        var centerY = (int)(sv_MainCanvas.Height - container.Height / 2);
                        idea.CenterX = centerX;
                        idea.CenterY = centerY;
                    }
                    sv_MainCanvas.Items.Add(container);
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
                        addedIdeaUi.startJustAddedAnimation(container.Orientation);
                    }
                    addedIdeaUi.noteUITranslatedEventHandler += new NoteUITranslatedEvent(noteUIManipluatedEventHandler);
                    addedIdeaUi.noteUIDeletedEventHandler += new NoteUIDeletedEvent(noteUIDeletedEventHandler);
                    addedIdeaUi.noteUISizeChangedListener += new NoteUISizeChangedEvent(addedIdeaUI_noteUISizeChangedListener);
                    addedIdeaUi.colorPaletteLaunchedEventHandler += new ColorPaletteLaunchedEvent(addedIdeaUI_colorPaletteLaunchedEventHandler);
                }
                (await _eventLogger).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteAdded(idea));
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        void addedIdeaUI_colorPaletteLaunchedEventHandler(object sender, float posX, float posY)
        {
            var colorPalette = new PostItColorPalette();
            colorPalette.setSize((sender as Control).Width, (sender as Control).Height);
            colorPalette.CallingControl = (Control)sender;
            colorPalette.colorPickedEventHandler += new ColorPickedEvent(colorPalette_colorPickedEventHandler);
            colorPalette.selectedColorApprovedHandler += new SelectedColorApproved(colorPalette_selectedColorApprovedHandler);
            mainGrid.Children.Add(colorPalette);
            var paletteMargin = colorPalette.Margin;
            paletteMargin.Left = posX - colorPalette.Width / 2;
            paletteMargin.Top = posY - colorPalette.Height / 2;
            colorPalette.Margin = paletteMargin;
            colorPalette.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            colorPalette.VerticalAlignment = System.Windows.VerticalAlignment.Top;
        }

        void colorPalette_colorPickedEventHandler(Control callingControl, string colorCode)
        {
            var imageBasedPostItUi = callingControl as ImageBasedPostItUI;
            imageBasedPostItUi?.setBackgroundPostItColor(colorCode);
        }

        async void colorPalette_selectedColorApprovedHandler(object sender, Control callingControl, string approvedColorCode)
        {

            var palette = (sender as PostItColorPalette);
            var postItUi = (callingControl as ImageBasedPostItUI);
            if (postItUi != null && postItUi.getLatestApprovedtBackgroundColor().CompareTo(approvedColorCode) != 0)
            {
                postItUi.approvedNewBackgroundColor(approvedColorCode);
                await TakeASnapshot();
                _brainstormManager.ChangeIdeaUIColor(postItUi.getAssociatedIdea().Id, approvedColorCode);
            }
            mainGrid.Children.Remove(sender as PostItColorPalette);
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
                    newStroke.DrawingAttributes = DrawingCanvasModeSwitcher.normalDrawingAttribute.Clone();
                    newStroke.DrawingAttributes.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(ideaData.StrokeColorCode);
                }
                else
                {
                    newStroke.DrawingAttributes = new DrawingAttributes();
                    newStroke.DrawingAttributes.Color = System.Windows.Media.Color.FromRgb(0, 0, 0);
                    newStroke.DrawingAttributes.Width = newStroke.DrawingAttributes.Height = 30;
                }
                drawingCanvas.Strokes.Add(newStroke);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        private ScatterViewItem FindNoteContainerOfIdea(IdeationUnit idea)
        {
            ScatterViewItem matchedItem = null;
            foreach (ScatterViewItem item in sv_MainCanvas.Items)
            {
                var noteUi = (IPostItUI)item.Content;
                if (noteUi.getNoteID() == idea.Id)
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
                    var noteUi = (IPostItUI)noteContainer.Content;
                    noteUi.update(ideaToUpdate);
                    noteContainer.Content = noteUi;
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }), new object[] { updatedIdea.Clone() });
        }

        private async Task UpdateNoteUiPosition(GenericIdeationObjects.IdeationUnit updatedIdea)
        {

            try
            {
                var noteContainer = FindNoteContainerOfIdea(updatedIdea);
                if (noteContainer != null)
                {
                    noteContainer.Center = new System.Windows.Point(updatedIdea.CenterX, updatedIdea.CenterY);
                    (await _eventLogger).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteMoved(updatedIdea));
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }

        }

        private async void RemoveNoteUi(IdeationUnit associatedIdea)
        {

            try
            {
                var ideaContainer = FindNoteContainerOfIdea(associatedIdea);
                sv_MainCanvas.Items.Remove(ideaContainer);
                (await _eventLogger).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteDeleted(associatedIdea));
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }

        }

        private void noteUIDeletedEventHandler(object sender, IdeationUnit associatedIdea)
        {
            _brainstormManager.RemoveIdea(associatedIdea);
        }

        async void addedIdeaUI_noteUISizeChangedListener(object sender, IdeationUnit associatedIdea, float scaleX, float scaleY)
        {
            await TakeASnapshot();
            (await _eventLogger).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteSizeChanged(associatedIdea, scaleX, scaleY));
        }

        private async void noteUIManipluatedEventHandler(object sender, IdeationUnit underlyingIdea, float newX, float newY)
        {
            //auto adjust to align postit note vertically
            var container = FindNoteContainerOfIdea(underlyingIdea);
            if (Math.Abs(container.Orientation) <= 20)
            {
                container.Orientation = 0;
            }
            await TakeASnapshot();
            _brainstormManager.UpdateIdeaPosition(underlyingIdea.Id, newX, newY);
        }

        private async void noteUpdateScheduler_updateEventHandler()
        {
            await _generalNoteDownloader.UpdateNotes();
            //await _anotoNotesDownloader.UpdateNotes();
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
            sv_MainCanvas.Width = this.ActualWidth;
            sv_MainCanvas.Height = this.ActualHeight;
            drawingCanvas.Width = this.ActualWidth;
            drawingCanvas.Height = this.ActualHeight;
            canvasesContainer.Width = this.ActualWidth;
            canvasesContainer.Height = this.ActualHeight;
            mainGrid.Width = this.ActualWidth;
            mainGrid.Height = this.ActualHeight;

            LayoutMenu();

            Loaded -= BrainstormCanvas_Loaded;
        }

        private async void brainstormManager_ideaAddedEventHandler(GenericIdeationObjects.IdeationUnit addedIdea)
        {
            // lock (sync)
            {
                var oneItemList = new List<IdeationUnit>();
                oneItemList.Add(addedIdea);
                AddNewIdeaUIs(oneItemList, true);
                if (!_generalNoteDownloader.InitializationFinished)
                    return;

                await TakeASnapshot();
                _timelineManager.AddADDChange(addedIdea);

            }
        }

        private async void brainstormManager_ideaCollectionRollBackFinishedEventHandler(List<IdeationUnit> currentIdeas)
        {
            // lock (sync)
            {
                ClearNotes();
                AddNewIdeaUIs(currentIdeas, false);
                recycleBin.RefreshNewDiscardedIdeasList(currentIdeas);
                (await _eventLogger).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_TimelineFrameFinishRetrieving());
                await TakeASnapshot();
            }
        }

        private async void brainstormManager_ideaRemovedHandler(GenericIdeationObjects.IdeationUnit removedIdea)
        {
            // lock (sync)
            {
                RemoveNoteUi(removedIdea);
                await TakeASnapshot();
                _timelineManager.AddDELETEChange(removedIdea.Id);
            }
        }

        private async void brainstormManager_ideaRestoredHandler(IdeationUnit restoredIdea)
        {
            // lock (sync)
            {
                var oneItemList = new List<IdeationUnit>();
                oneItemList.Add(restoredIdea);
                AddNewIdeaUIs(oneItemList, false);
                await TakeASnapshot();
                _timelineManager.AddRESTOREChange(restoredIdea.Id);
                (await _eventLogger).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteRestored(restoredIdea));
            }
        }

        private async void brainstormManager_ideaUpdatedHandler(GenericIdeationObjects.IdeationUnit updatedIdea, GenericIdeationObjects.IdeationUnit.IdeaUpdateType updateType)
        {
            // lock (sync)
            {
                switch (updateType)
                {
                    case IdeationUnit.IdeaUpdateType.Position:
                        await UpdateNoteUiPosition(updatedIdea);
                        break;

                    case IdeationUnit.IdeaUpdateType.Content:
                        UpdateNoteUiContent(updatedIdea);
                        break;
                }
                await TakeASnapshot();
                switch (updateType)
                {
                    case IdeationUnit.IdeaUpdateType.Position:
                        _timelineManager.AddUPDATEPositionChange(updatedIdea.Id, updatedIdea.CenterX, updatedIdea.CenterY);
                        break;

                    case IdeationUnit.IdeaUpdateType.Content:
                        _timelineManager.AddUPDATEContentChange(updatedIdea.Id, updatedIdea.Content);
                        break;
                }
            }
        }

        async void brainstormManager_ideaUIColorChangeHandler(IdeationUnit updatedIdea, string colorCode)
        {
            await TakeASnapshot();
            _timelineManager.AddCOLORChange(updatedIdea.Id, colorCode);
        }

        private void ClearNotes()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    sv_MainCanvas.Items.Clear();
                    sv_MainCanvas.UpdateLayout();
                    drawingCanvas.Strokes.Clear();
                    drawingCanvas.UpdateLayout();
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }));
        }

        private async void drawingCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            try
            {
                sv_MainCanvas.IsHitTestVisible = true;
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
                strokeIdea.Id = IdeaIDGenerator.generateID();
                var strokeData = new StrokeData();
                strokeData.IsErasingStroke = DrawingCanvasModeSwitcher.IsInErasingMode();
                strokeData.StrokeColorCode = new System.Windows.Media.ColorConverter().ConvertToString(latestStroke.DrawingAttributes.Color);
                strokeData.StrokePoints = pathPoints;
                strokeIdea.Content = strokeData;
                _brainstormManager.AddIdeaInBackground(strokeIdea);
                //get the current screenshot
                await TakeASnapshot();
                _timelineManager.AddADDChange(strokeIdea);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        private void InitBrainstormingProcessors()
        {
            _brainstormManager = new PostItGeneralManager();
            _brainstormManager.ideaAddedEventHandler += new PostItGeneralManager.NewIdeaAddedEvent(brainstormManager_ideaAddedEventHandler);
            _brainstormManager.ideaUpdatedHandler += new PostItGeneralManager.IdeaUpdatedEvent(brainstormManager_ideaUpdatedHandler);
            _brainstormManager.ideaRemovedHandler += new PostItGeneralManager.IdeaRemovedEvent(brainstormManager_ideaRemovedHandler);
            _brainstormManager.ideaRestoredHandler += new PostItGeneralManager.IdeaRestoredEvent(brainstormManager_ideaRestoredHandler);
            _brainstormManager.ideaUIColorChangeHandler += new PostItGeneralManager.IdeaUIColorChangeEvent(brainstormManager_ideaUIColorChangeHandler);
            _brainstormManager.ideaCollectionRollBackFinishedEventHandler += new PostItGeneralManager.IdeaCollectionRollBackFinished(brainstormManager_ideaCollectionRollBackFinishedEventHandler);

            _brainstormManager.TrashManager.discardedIdeaReceivedEventHandler += new Recycle_Bin.RecycleBinManager.DiscardedIdeaReceived(recycleBin.AddDiscardedIdea);

            recycleBin.noteRestoredEventHandler += new Recycle_Bin.RecycleBinManager.DiscardedIdeaRestored(_brainstormManager.TrashManager.RestoreIdea);
        }

        private async Task InitNetworkCommManager()
        {
            //processors related to cloud service
            _generalNoteDownloader = new NoteUpdater();

            _anotoNotesDownloader = new AnotoNoteUpdater();
            _noteUpdateScheduler = new NoteUpdateScheduler();

            _cloudDataEventProcessor = new CloudDataEventProcessor();
            _generalNoteDownloader.NoteStreamsDownloadedHandler += new NewNoteStreamsDownloaded(_cloudDataEventProcessor.handleDownloadedStreamsFromCloud);
            _anotoNotesDownloader.NoteStreamsDownloadedHandler += new NewNoteStreamsDownloaded(_cloudDataEventProcessor.handleDownloadedStreamsFromCloud);
            _cloudDataEventProcessor.newNoteExtractedEventHandler += new CloudDataEventProcessor.NewNoteExtractedFromStreamEvent(_brainstormManager.HandleComingIdea);
            await _generalNoteDownloader.UpdateNotes();
            _noteUpdateScheduler.updateEventHandler += new NoteUpdateScheduler.UpdateIntervalTickedEvent(noteUpdateScheduler_updateEventHandler);

            _p2PClient = new AsyncTCPClient();
            _remotePointerManager = new RemotePointerManager();
            _p2PClient.setP2PDataListener(_remotePointerManager);
            _remotePointerManager.setPointerEventListener(this);

            _p2PClient.StartClient();
        }

        private void InitTimeline()
        {
            timelineView.frameSelectedEventHandler += new BrainstormingTimelineUI.TimelineFrameSelected(timelineView_frameSelectedEventHandler);
            _timelineManager = new TimelineControllers.TimelineChangeManager();
            _timelineManager.newTimelineFrameAddedEventHandler += new TimelineControllers.TimelineChangeManager.NewTimelineFrameAdded(newTimelineFrameAddedEventHandler);
            _timelineManager.startEnumeratingEventHandler += new TimelineControllers.TimelineChangeManager.StartEnumeratingFromBeginning(_brainstormManager.reset);
            _timelineManager.finishEnumeratingEventHandler += new TimelineControllers.TimelineChangeManager.FinishEnumeratingToTheSelected(_brainstormManager.notifyIdeaCollectionRollBack);

            var eventIntepreter = new TimelineControllers.TimlineEventIntepreter();
            eventIntepreter.ADDeventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.ADDIdeaCommandExtracted(_brainstormManager.AddIdeaInBackground);
            eventIntepreter.REMOVEeventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.REMOVEIdeaCommandExtracted(_brainstormManager.RemoveIdeaInBackground);
            eventIntepreter.RESTOREeventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.RESTOREIdeaCommandExtraced(_brainstormManager.RestoreIdeaInBackground);
            eventIntepreter.UPDATEPosEventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.UPDATEIdeaPositionCommandExtracted(_brainstormManager.UpdateIdeaPositionInBackground);
            eventIntepreter.UPDATEContentEventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.UPDATEIdeaContentCommandExtracted(_brainstormManager.UpdateIdeaContentInBackground);
            eventIntepreter.COLORChangeEventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.COLORChangeCommandExtracted(_brainstormManager.ChangeIdeaUIColorInBackground);
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
                    drawingCanvas.UsesTouchShape = false;
                    drawingCanvas.DefaultDrawingAttributes.Color = System.Windows.Media.Color.FromRgb(0, 0, 0);
                    drawingCanvas.DefaultDrawingAttributes.Width = drawingCanvas.DefaultDrawingAttributes.Height = 30;
                    menuItem_DrawingSwitch.Header = MainWindow.Resources["PencilIcon"];
                }
                else
                {
                    drawingCanvas.UsesTouchShape = true;
                    drawingCanvas.DefaultDrawingAttributes = DrawingCanvasModeSwitcher.normalDrawingAttribute.Clone();
                    menuItem_DrawingSwitch.Header = MainWindow.Resources["EraserIcon"];
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        private void menuItem_RecycleBin_Click(object sender, RoutedEventArgs e)
        {
            if (timelineView.Visibility == System.Windows.Visibility.Visible)
            {
                timelineView.FadeOut();
            }
            if (recycleBin.Visibility == System.Windows.Visibility.Visible)
            {
                recycleBin.FadeOut();
            }
            else
            {
                recycleBin.FadeIn();
            }
        }

        private void menuItem_Timeline_Click(object sender, RoutedEventArgs e)
        {
            if (recycleBin.Visibility == System.Windows.Visibility.Visible)
            {
                recycleBin.FadeOut();
            }
            if (timelineView.Visibility == System.Windows.Visibility.Hidden)
            {
                timelineView.FadeIn();
            }
            else
            {
                timelineView.FadeOut();
            }
        }

        private void newTimelineFrameAddedEventHandler(TimelineControllers.TimelineFrame addedFrame)
        {
            this.Dispatcher.Invoke(new Action<TimelineControllers.TimelineFrame>((frameToAdd) =>
            {
                timelineView.AddFrame(frameToAdd);
            }), new TimelineControllers.TimelineFrame[] { addedFrame });
        }
        private async void timelineView_frameSelectedEventHandler(int selectedFrameId)
        {
            (await _eventLogger).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_TimelineFrameStartRetrieving(selectedFrameId));
            _timelineManager.SelectFrame(selectedFrameId);
        }
        #endregion



        private void RefreshBrainstormingWhiteboard()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                sv_MainCanvas.UpdateLayout();
            }));
        }


        private void sv_MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            sv_MainCanvas.IsHitTestVisible = false;
        }

        private void sv_MainCanvas_TouchDown(object sender, TouchEventArgs e)
        {
            base.OnTouchDown(e);
            sv_MainCanvas.IsHitTestVisible = false;
        }
        #region screenshot related
        private async Task TakeASnapshot()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    sv_MainCanvas.UpdateLayout();
                    drawingCanvas.UpdateLayout();
                    canvasesContainer.UpdateLayout();
                    double dpi = 96;
                    //prepare to render the notes
                    var noteContainerRenderTargetBitmap = new RenderTargetBitmap((int)canvasesContainer.ActualWidth, (int)canvasesContainer.ActualHeight, dpi, dpi, PixelFormats.Pbgra32);
                    noteContainerRenderTargetBitmap.Render(canvasesContainer);
                    var noteContainerImgSrc = (ImageSource)noteContainerRenderTargetBitmap.Clone();
                    var resizedNoteContainerBmpFrame = Utilities.UtilitiesLib.CreateResizedBitmapFrame(noteContainerImgSrc, (int)(canvasesContainer.ActualWidth * 3 / 4), (int)(canvasesContainer.Height * 3 / 4), 0);
                    var imageEncoder = new PngBitmapEncoder();
                    imageEncoder.Frames.Add(BitmapFrame.Create(resizedNoteContainerBmpFrame));
                    //imageEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                    var screenshotBytes = new byte[1];
                    using (var stream = new MemoryStream())
                    {
                        imageEncoder.Save(stream);
                        stream.Seek(0, 0);
                        screenshotBytes = stream.ToArray();
                        Utilities.GlobalObjects.currentScreenshotBytes = screenshotBytes;

                        var boardScreenUpdater =
                            (await BoardScreenUpdater.GetInstance(_generalNoteDownloader.Storage));

                        await boardScreenUpdater.UpdateMetaplanBoardScreen(new MemoryStream(screenshotBytes));

                        //bgUploader.RunWorkerAsync(new MemoryStream(screenshotBytes));
                    }

                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            });


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
            var header = AsyncTCPClient.createPacketHeader(dataToSend);
            _p2PClient.SyncSend(header);
            _p2PClient.SyncSend(dataToSend);
        }
        #endregion screenshot related


        #region Remote Pointer Related

        public async void NewPointerAddedEvent(RemotePointer addedPointer, string assignedColorCode)
        {

            try
            {
                var pointerContainer = new ScatterViewItem();
                pointerContainer.ApplyTemplate();
                var pointerUi = new RemotePointerUI();
                pointerUi.PointerID = addedPointer.Id;
                pointerUi.setPointerColor(assignedColorCode);
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
                sv_RemotePointerCanvas.Items.Add(pointerContainer);
                var x = (int)(addedPointer.X * canvasesContainer.Width);
                var y = (int)(addedPointer.Y * canvasesContainer.Height);
                pointerContainer.Center = new System.Windows.Point(x, y);
                sv_RemotePointerCanvas.UpdateLayout();

                (await _eventLogger).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_RemotePointerAdded(addedPointer));
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }

        }
        public async void PointerUpdatedEvent(RemotePointer updatedPointer)
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
                    sv_RemotePointerCanvas.UpdateLayout();
                    pointerContainer.Tag = updatedPointer;
                    (await _eventLogger).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_RemotePointerLeft(updatedPointer));
                }
                else
                {
                    //make pointer fade in
                    if (!((RemotePointer)pointerContainer.Tag).IsActive)
                    {
                        var anim = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2));
                        pointerContainer.BeginAnimation(UserControl.OpacityProperty, anim);
                        (await _eventLogger).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_RemotePointerReentered(updatedPointer));
                    }
                    //update location
                    var x = (int)(updatedPointer.X * canvasesContainer.Width);
                    var y = (int)(updatedPointer.Y * canvasesContainer.Height);
                    pointerContainer.Center = new System.Windows.Point(x, y);
                    if (((RemotePointer)pointerContainer.Tag).IsActive)
                    {
                        (await _eventLogger).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_RemotePointerMoved(updatedPointer));
                    }
                    pointerContainer.Tag = updatedPointer;
                    sv_RemotePointerCanvas.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }

        }
        ScatterViewItem FindViewItemWithPointerId(int pointerId)
        {
            ScatterViewItem matchedItem = null;
            foreach (ScatterViewItem item in sv_RemotePointerCanvas.Items)
            {
                var remotePointer = (RemotePointerUI)(item.Content);
                if (remotePointer.PointerID == pointerId)
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

        private NoteUpdater _generalNoteDownloader = null;


        private AsyncTCPClient _p2PClient = null;
        private RemotePointerManager _remotePointerManager = null;
        //Network data processors
        private NoteUpdateScheduler _noteUpdateScheduler = null;

        //PostItNetworkDataManager networkDataManager = null;
        //Timeline processors
        private TimelineControllers.TimelineChangeManager _timelineManager;
        private Task<BrainstormingEventLogger> _eventLogger;
        private object _lock = new object();

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
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Maximized;
            this.WindowStyle = System.Windows.WindowStyle.None;
            (await _eventLogger).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_Start());
        }

        private async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            (await _eventLogger).Close();
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
                _generalNoteDownloader.Close();
                _anotoNotesDownloader.Close();

            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        private async void MainWindow_Initialized(object sender, EventArgs e)
        {
            // Add handlers for window availability events
            _eventLogger = Utilities.BrainstormingEventLogger.GetInstance(new NoteUpdater().Storage);
            AddWindowAvailabilityHandlers();
            InitBrainstormingProcessors();
            await InitNetworkCommManager();

            InitTimeline();

            DrawingCanvasModeSwitcher.normalDrawingAttribute = drawingCanvas.DefaultDrawingAttributes.Clone();

            var workingArea = System.Windows.Forms.Screen.AllScreens[Properties.Settings.Default.ActiveWorkingScreen].WorkingArea;
            this.Left = workingArea.Left;
            this.Top = workingArea.Top;
            this.Width = workingArea.Width;
            this.Height = workingArea.Height;


        }
        #endregion window system events
        /* Runs on UI thread */
    }
}