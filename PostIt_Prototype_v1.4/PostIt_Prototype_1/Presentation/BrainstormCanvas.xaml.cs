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
            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
            InitBrainstormingProcessors();
            InitNetworkCommManager();
            InitTimeline();

            DrawingCanvasModeSwitcher.normalDrawingAttribute = drawingCanvas.DefaultDrawingAttributes.Clone();

            System.Drawing.Rectangle workingArea = System.Windows.Forms.Screen.AllScreens[Properties.Settings.Default.ActiveWorkingScreen].WorkingArea;
            this.Left = workingArea.Left;
            this.Top = workingArea.Top;
            this.Width = workingArea.Width;
            this.Height = workingArea.Height;
        }


        #endregion Public Constructors

        #region Private Methods

        private void addMessageToListBox(string message)
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
        private void addNewIdeaUIs(List<IdeationUnit> ideas, bool asInit)
        {
            Random rnd = new Random();

            Dispatcher.Invoke(new Action<List<IdeationUnit>, bool>((ideasToAdd, init) =>
            {
                foreach (IdeationUnit idea in ideasToAdd)
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
                sv_MainCanvas.UpdateLayout();
            }), new object[] { ideas, asInit });
        }

        private void AddSingleIdeaGroup(IdeationUnit ideaGroup)
        {
            try
            {
                IdeaGroupUI groupUI = new IdeaGroupUI();
                groupUI.setNoteID(ideaGroup.Id);
                groupUI.Tag = ideaGroup;
                groupUI.update(ideaGroup);
                ScatterViewItem container = new ScatterViewItem();
                container.Content = groupUI;
                container.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                container.Width = groupUI.Width;
                container.Height = groupUI.Height;
                container.Orientation = 0;
                container.CanScale = false;
                container.CanRotate = false;
                container.Center = new System.Windows.Point(ideaGroup.CenterX, ideaGroup.CenterY);
                groupUI.InitContainer(container);
                sv_MainCanvas.Items.Add(container);
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
                IPostItUI addedIdeaUI = null;
                ScatterViewItem container = new ScatterViewItem();
                PostItNote castNote = (PostItNote)idea;
                if (castNote.Content is Bitmap)
                {
                    ImageBasedPostItUI noteUI = new ImageBasedPostItUI();

                    noteUI.Tag = idea;
                    noteUI.setNoteID(castNote.Id);
                    noteUI.update(idea);
                    noteUI.Width = noteUI.InitWidth;
                    noteUI.Height = noteUI.InitHeight;
                    if (castNote.MetaData.UiBackgroundColor.Length > 0)
                    {
                        noteUI.setBackgroundPostItColor(castNote.MetaData.UiBackgroundColor);
                        noteUI.approvedNewBackgroundColor(castNote.MetaData.UiBackgroundColor);
                    }

                    container.Content = noteUI;
                    container.Width = noteUI.Width;
                    container.Height = noteUI.Height;

                    if (idea.CenterX == 0 && idea.CenterY == 0)
                    {
                        int centerX = (int)(container.Width / 2);
                        int centerY = (int)(sv_MainCanvas.Height - container.Height / 2);
                        idea.CenterX = centerX;
                        idea.CenterY = centerY;
                    }
                    sv_MainCanvas.Items.Add(container);
                    container.Center = new System.Windows.Point(idea.CenterX, idea.CenterY);
                    container.Orientation = initAngle;
                    container.ZIndex = 1;
                    container.CanScale = false;
                    addedIdeaUI = noteUI;
                    addedIdeaUI.InitContainer(container);
                }
                if (addedIdeaUI != null)
                {
                    if (init)
                    {
                        addedIdeaUI.startJustAddedAnimation(container.Orientation);
                    }
                    addedIdeaUI.noteUITranslatedEventHandler += new NoteUITranslatedEvent(noteUIManipluatedEventHandler);
                    addedIdeaUI.noteUIDeletedEventHandler += new NoteUIDeletedEvent(noteUIDeletedEventHandler);
                    addedIdeaUI.noteUISizeChangedListener += new NoteUISizeChangedEvent(addedIdeaUI_noteUISizeChangedListener);
                    addedIdeaUI.colorPaletteLaunchedEventHandler += new ColorPaletteLaunchedEvent(addedIdeaUI_colorPaletteLaunchedEventHandler);
                }
                Utilities.BrainstormingEventLogger.GetInstance(dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteAdded(idea));
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        void addedIdeaUI_colorPaletteLaunchedEventHandler(object sender, float posX, float posY)
        {
            PostItColorPalette colorPalette = new PostItColorPalette();
            colorPalette.setSize((sender as Control).Width, (sender as Control).Height);
            colorPalette.CallingControl = (Control)sender;
            colorPalette.colorPickedEventHandler += new ColorPickedEvent(colorPalette_colorPickedEventHandler);
            colorPalette.selectedColorApprovedHandler += new SelectedColorApproved(colorPalette_selectedColorApprovedHandler);
            mainGrid.Children.Add(colorPalette);
            Thickness paletteMargin = colorPalette.Margin;
            paletteMargin.Left = posX - colorPalette.Width / 2;
            paletteMargin.Top = posY - colorPalette.Height / 2;
            colorPalette.Margin = paletteMargin;
            colorPalette.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            colorPalette.VerticalAlignment = System.Windows.VerticalAlignment.Top;
        }

        void colorPalette_colorPickedEventHandler(Control callingControl, string colorCode)
        {
            (callingControl as ImageBasedPostItUI).setBackgroundPostItColor(colorCode);

        }

        void colorPalette_selectedColorApprovedHandler(object sender,Control callingControl,string approvedColorCode)
        {

            PostItColorPalette palette = (sender as PostItColorPalette);
            ImageBasedPostItUI postItUI = (callingControl as ImageBasedPostItUI);
            if (postItUI.getLatestApprovedtBackgroundColor().CompareTo(approvedColorCode) != 0)
            {
                postItUI.approvedNewBackgroundColor(approvedColorCode);
                TakeASnapshot();
                brainstormManager.ChangeIdeaUIColor(postItUI.getAssociatedIdea().Id, approvedColorCode);
            }
            mainGrid.Children.Remove(sender as PostItColorPalette);
        }
        private void AddSingleStrokeBasedNote(IdeationUnit strokeBasedIdea)
        {
            try
            {
                StrokeData ideaData = (StrokeData)(strokeBasedIdea.Content);
                List<System.Windows.Point> strokePoints = ideaData.StrokePoints;
                StylusPointCollection stylusPoints = new StylusPointCollection();
                foreach (System.Windows.Point p in strokePoints)
                {
                    StylusPoint stylusP = new StylusPoint(p.X, p.Y);
                    stylusPoints.Add(stylusP);
                }
                Stroke newStroke = new Stroke(stylusPoints);
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

        private ScatterViewItem findNoteContainerOfIdea(IdeationUnit idea)
        {
            ScatterViewItem matchedItem = null;
            foreach (ScatterViewItem item in sv_MainCanvas.Items)
            {
                IPostItUI noteUI = (IPostItUI)item.Content;
                if (noteUI.getNoteID() == idea.Id)
                {
                    matchedItem = item;
                }
            }
            return matchedItem;
        }

        private void updateNoteUIContent(GenericIdeationObjects.IdeationUnit updatedIdea)
        {
            this.Dispatcher.Invoke(new Action<IdeationUnit>((ideaToUpdate) =>
            {
                try
                {
                    ScatterViewItem noteContainer = findNoteContainerOfIdea(ideaToUpdate);
                    IPostItUI noteUI = (IPostItUI)noteContainer.Content;
                    noteUI.update(ideaToUpdate);
                    noteContainer.Content = noteUI;
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }), new object[] { updatedIdea.Clone() });
        }

        private void updateNoteUIPosition(GenericIdeationObjects.IdeationUnit updatedIdea)
        {
            this.Dispatcher.Invoke(new Action<IdeationUnit>((ideaToUpdate) =>
            {
                try
                {
                    ScatterViewItem noteContainer = findNoteContainerOfIdea(ideaToUpdate);
                    if (noteContainer != null)
                    {
                        noteContainer.Center = new System.Windows.Point(ideaToUpdate.CenterX, ideaToUpdate.CenterY);
                        Utilities.BrainstormingEventLogger.GetInstance(dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteMoved(ideaToUpdate));
                    }
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }), new object[] { updatedIdea });
        }

        private void removeNoteUI(IdeationUnit associatedIdea)
        {
            this.Dispatcher.Invoke(new Action<IdeationUnit>((ideaToDel) =>
            {
                try
                {
                    ScatterViewItem ideaContainer = findNoteContainerOfIdea(ideaToDel);
                    sv_MainCanvas.Items.Remove(ideaContainer);
                    Utilities.BrainstormingEventLogger.GetInstance(dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteDeleted(associatedIdea));
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }), new object[] { associatedIdea });
        }

        private void noteUIDeletedEventHandler(object sender, IdeationUnit associatedIdea)
        {
            brainstormManager.RemoveIdea(associatedIdea);
        }

        void addedIdeaUI_noteUISizeChangedListener(object sender, IdeationUnit associatedIdea, float scaleX, float scaleY)
        {
            TakeASnapshot();
            Utilities.BrainstormingEventLogger.GetInstance(dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteSizeChanged(associatedIdea, scaleX, scaleY));
        }

        private void noteUIManipluatedEventHandler(object sender, IdeationUnit underlyingIdea, float newX, float newY)
        {
            TakeASnapshot();
            brainstormManager.UpdateIdeaPosition(underlyingIdea.Id, newX, newY);
        }

        private void noteUpdateScheduler_updateEventHandler()
        {
            //Thread dropboxImageNoteUpdateThread = new Thread(new ThreadStart(dropboxGeneralNoteDownloader.UpdateNotes));
            //dropboxImageNoteUpdateThread.Start();
            dropboxGeneralNoteDownloader.UpdateNotes();

            //Thread anotoUpdateThread = new Thread(new ThreadStart(anotoNotesDownloader.UpdateNotes));
            //anotoUpdateThread.Start();
            anotoNotesDownloader.UpdateNotes();
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

            layoutMenu();

            Loaded -= BrainstormCanvas_Loaded;
        }

        private void brainstormManager_ideaAddedEventHandler(GenericIdeationObjects.IdeationUnit addedIdea)
        {
            // lock (sync)
            {
                List<IdeationUnit> oneItemList = new List<IdeationUnit>();
                oneItemList.Add(addedIdea);
                addNewIdeaUIs(oneItemList, true);
                TakeASnapshot();
                timelineManager.AddADDChange(addedIdea);
            }
        }

        private void brainstormManager_ideaCollectionRollBackFinishedEventHandler(List<IdeationUnit> currentIdeas)
        {
            // lock (sync)
            {
                clearNotes();
                addNewIdeaUIs(currentIdeas, false);
                recycleBin.RefreshNewDiscardedIdeasList(currentIdeas);
                TakeASnapshot();
            }
        }

        private void brainstormManager_ideaRemovedHandler(GenericIdeationObjects.IdeationUnit removedIdea)
        {
            // lock (sync)
            {
                removeNoteUI(removedIdea);
                TakeASnapshot();
                timelineManager.AddDELETEChange(removedIdea.Id);
            }
        }

        private void brainstormManager_ideaRestoredHandler(IdeationUnit restoredIdea)
        {
            // lock (sync)
            {
                List<IdeationUnit> oneItemList = new List<IdeationUnit>();
                oneItemList.Add(restoredIdea);
                addNewIdeaUIs(oneItemList, false);
                TakeASnapshot();
                timelineManager.AddRESTOREChange(restoredIdea.Id);
                Utilities.BrainstormingEventLogger.GetInstance(dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_NoteRestored(restoredIdea));
            }
        }

        private void brainstormManager_ideaUpdatedHandler(GenericIdeationObjects.IdeationUnit updatedIdea, GenericIdeationObjects.IdeationUnit.IdeaUpdateType updateType)
        {
            // lock (sync)
            {
                switch (updateType)
                {
                    case IdeationUnit.IdeaUpdateType.Position:
                        updateNoteUIPosition(updatedIdea);
                        break;

                    case IdeationUnit.IdeaUpdateType.Content:
                        updateNoteUIContent(updatedIdea);
                        break;
                }
                TakeASnapshot();
                switch (updateType)
                {
                    case IdeationUnit.IdeaUpdateType.Position:
                        timelineManager.AddUPDATEPositionChange(updatedIdea.Id, updatedIdea.CenterX, updatedIdea.CenterY);
                        break;

                    case IdeationUnit.IdeaUpdateType.Content:
                        timelineManager.AddUPDATEContentChange(updatedIdea.Id, updatedIdea.Content);
                        break;
                }
            }
        }

        void brainstormManager_ideaUIColorChangeHandler(IdeationUnit updatedIdea, string colorCode)
        {
            TakeASnapshot();
            timelineManager.AddCOLORChange(updatedIdea.Id, colorCode);
        }

        private void clearNotes()
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

        private void drawingCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            try
            {
                sv_MainCanvas.IsHitTestVisible = true;
                Stroke latestStroke = e.Stroke;
                StylusPointCollection strokePoints = latestStroke.StylusPoints;
                if (!DrawingCanvasModeSwitcher.IsInErasingMode() && strokePoints.Count < 10)
                {
                    return;
                }
                List<System.Windows.Point> pathPoints = new List<System.Windows.Point>();
                foreach (StylusPoint stylusP in strokePoints)
                {
                    System.Windows.Point p = new System.Windows.Point();
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
                StrokeData strokeData = new StrokeData();
                strokeData.IsErasingStroke = DrawingCanvasModeSwitcher.IsInErasingMode();
                strokeData.StrokeColorCode = new System.Windows.Media.ColorConverter().ConvertToString(latestStroke.DrawingAttributes.Color);
                strokeData.StrokePoints = pathPoints;
                strokeIdea.Content = strokeData;
                brainstormManager.AddIdeaInBackground(strokeIdea);
                //get the current screenshot
                TakeASnapshot();
                timelineManager.AddADDChange(strokeIdea);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        private void InitBrainstormingProcessors()
        {
            brainstormManager = new PostItGeneralManager();
            brainstormManager.ideaAddedEventHandler += new PostItGeneralManager.NewIdeaAddedEvent(brainstormManager_ideaAddedEventHandler);
            brainstormManager.ideaUpdatedHandler += new PostItGeneralManager.IdeaUpdatedEvent(brainstormManager_ideaUpdatedHandler);
            brainstormManager.ideaRemovedHandler += new PostItGeneralManager.IdeaRemovedEvent(brainstormManager_ideaRemovedHandler);
            brainstormManager.ideaRestoredHandler += new PostItGeneralManager.IdeaRestoredEvent(brainstormManager_ideaRestoredHandler);
            brainstormManager.ideaUIColorChangeHandler += new PostItGeneralManager.IdeaUIColorChangeEvent(brainstormManager_ideaUIColorChangeHandler);
            brainstormManager.ideaCollectionRollBackFinishedEventHandler += new PostItGeneralManager.IdeaCollectionRollBackFinished(brainstormManager_ideaCollectionRollBackFinishedEventHandler);

            brainstormManager.TrashManager.discardedIdeaReceivedEventHandler += new Recycle_Bin.RecycleBinManager.DiscardedIdeaReceived(recycleBin.AddDiscardedIdea);

            recycleBin.noteRestoredEventHandler += new Recycle_Bin.RecycleBinManager.DiscardedIdeaRestored(brainstormManager.TrashManager.RestoreIdea);
        }

        private void InitNetworkCommManager()
        {
            //processors related to cloud service
            dropboxGeneralNoteDownloader = new NoteUpdater();
            anotoNotesDownloader = new AnotoNoteUpdater();
            noteUpdateScheduler = new NoteUpdateScheduler();
            noteUpdateScheduler.updateEventHandler += new NoteUpdateScheduler.UpdateIntervalTickedEvent(noteUpdateScheduler_updateEventHandler);
            cloudDataEventProcessor = new CloudDataEventProcessor();
            dropboxGeneralNoteDownloader.noteStreamsDownloadedHandler += new NewNoteStreamsDownloaded(cloudDataEventProcessor.handleDownloadedStreamsFromCloud);
            anotoNotesDownloader.noteStreamsDownloadedHandler += new NewNoteStreamsDownloaded(cloudDataEventProcessor.handleDownloadedStreamsFromCloud);
            cloudDataEventProcessor.newNoteExtractedEventHandler += new CloudDataEventProcessor.NewNoteExtractedFromStreamEvent(brainstormManager.HandleComingIdea);

            p2pClient = new AsyncTCPClient();
            remotePointerManager = new RemotePointerManager();
            p2pClient.setP2PDataListener(remotePointerManager);
            remotePointerManager.setPointerEventListener(this);
            p2pClient.StartClient();
        }

        private void InitTimeline()
        {
            timelineView.frameSelectedEventHandler += new BrainstormingTimelineUI.TimelineFrameSelected(timelineView_frameSelectedEventHandler);
            timelineManager = new TimelineControllers.TimelineChangeManager();
            timelineManager.newTimelineFrameAddedEventHandler += new TimelineControllers.TimelineChangeManager.NewTimelineFrameAdded(newTimelineFrameAddedEventHandler);
            timelineManager.startEnumeratingEventHandler += new TimelineControllers.TimelineChangeManager.StartEnumeratingFromBeginning(brainstormManager.reset);
            timelineManager.finishEnumeratingEventHandler += new TimelineControllers.TimelineChangeManager.FinishEnumeratingToTheSelected(brainstormManager.notifyIdeaCollectionRollBack);

            TimelineControllers.TimlineEventIntepreter eventIntepreter = new TimelineControllers.TimlineEventIntepreter();
            eventIntepreter.ADDeventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.ADDIdeaCommandExtracted(brainstormManager.AddIdeaInBackground);
            eventIntepreter.REMOVEeventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.REMOVEIdeaCommandExtracted(brainstormManager.RemoveIdeaInBackground);
            eventIntepreter.RESTOREeventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.RESTOREIdeaCommandExtraced(brainstormManager.RestoreIdeaInBackground);
            eventIntepreter.UPDATEPosEventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.UPDATEIdeaPositionCommandExtracted(brainstormManager.UpdateIdeaPositionInBackground);
            eventIntepreter.UPDATEContentEventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.UPDATEIdeaContentCommandExtracted(brainstormManager.UpdateIdeaContentInBackground);
            eventIntepreter.COLORChangeEventExtractedHandler +=new TimelineControllers.TimlineEventIntepreter.COLORChangeCommandExtracted(brainstormManager.ChangeIdeaUIColorInBackground);
            timelineManager.EventIntepreter = eventIntepreter;
        }

        #region Menu related
        private void layoutMenu()
        {
            //MainMenu.Radius = this.Height * 0.15;
            MainMenu.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            MainMenu.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Thickness mainMenuMargin = MainMenu.Margin;
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

        #endregion



        private void refreshBrainstormingWhiteboard()
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
        private void TakeASnapshot()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    sv_MainCanvas.UpdateLayout();
                    drawingCanvas.UpdateLayout();
                    canvasesContainer.UpdateLayout();
                    double dpi = 96;
                    //prepare to render the notes
                    RenderTargetBitmap noteContainerRenderTargetBitmap = new RenderTargetBitmap((int)canvasesContainer.ActualWidth, (int)canvasesContainer.ActualHeight, dpi, dpi, PixelFormats.Pbgra32);
                    noteContainerRenderTargetBitmap.Render(canvasesContainer);
                    ImageSource NoteContainerImgSrc = (ImageSource)noteContainerRenderTargetBitmap.Clone();
                    BitmapFrame resizedNoteContainerBmpFrame = Utilities.UtilitiesLib.CreateResizedBitmapFrame(NoteContainerImgSrc, (int)(canvasesContainer.ActualWidth * 3 / 4), (int)(canvasesContainer.Height * 3 / 4), 0);
                    PngBitmapEncoder imageEncoder = new PngBitmapEncoder();
                    imageEncoder.Frames.Add(BitmapFrame.Create(resizedNoteContainerBmpFrame));
                    //imageEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                    byte[] screenshotBytes = new byte[1];
                    using (MemoryStream stream = new MemoryStream())
                    {
                        imageEncoder.Save(stream);
                        stream.Seek(0, 0);
                        screenshotBytes = stream.ToArray();
                        Utilities.GlobalObjects.currentScreenshotBytes = screenshotBytes;
                        //broadcastScreenshot(screenshotBytes);
                        Task.Factory.StartNew(() =>
                        {
                            BoardScreenUpdater.GetInstance(dropboxGeneralNoteDownloader.Storage).UpdateMetaplanBoardScreen(new MemoryStream(screenshotBytes));
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
        public void broadcastScreenshot(byte[] screenshotBytes)
        {
            byte[] prefix = Encoding.UTF8.GetBytes("<WB_SCREEN>");
            byte[] postfix = Encoding.UTF8.GetBytes("</WB_SCREEN>");
            byte[] dataToSend = new byte[prefix.Length + screenshotBytes.Length + postfix.Length];
            int index = 0;
            Array.Copy(prefix, 0, dataToSend, index, prefix.Length);
            index += prefix.Length;
            Array.Copy(screenshotBytes, 0, dataToSend, index, screenshotBytes.Length);
            index += screenshotBytes.Length;
            Array.Copy(postfix, 0, dataToSend, index, postfix.Length);
            //p2pClient.Send(dataToSend);
            byte[] header = AsyncTCPClient.createPacketHeader(dataToSend);
            p2pClient.SyncSend(header);
            p2pClient.SyncSend(dataToSend);
        }
        #endregion screenshot related
        private void timelineView_frameSelectedEventHandler(int selectedFrameID)
        {
            timelineManager.SelectFrame(selectedFrameID);
        }


        #region Remote Pointer Related

        public void NewPointerAddedEvent(RemotePointer addedPointer, string assignedColorCode)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    ScatterViewItem pointerContainer = new ScatterViewItem();
                    pointerContainer.ApplyTemplate();
                    RemotePointerUI pointerUI = new RemotePointerUI();
                    pointerUI.PointerID = addedPointer.Id;
                    pointerUI.setPointerColor(assignedColorCode);
                    pointerUI.Width = pointerUI.Height = 50;
                    pointerContainer.Width = pointerUI.Width;
                    pointerContainer.Height = pointerUI.Height;
                    //disable surrounding shadow
                    pointerContainer.Background = null;
                    pointerContainer.BorderThickness = new Thickness(0);
                    pointerContainer.ShowsActivationEffects = false;
                    //SurfaceShadowChrome ssc = pointerContainer.Template.FindName("shadow", pointerContainer) as SurfaceShadowChrome;
                    //ssc.Visibility = Visibility.Collapsed;
                    //add to the canvas and adjust location
                    pointerContainer.Content = pointerUI;
                    pointerContainer.Tag = addedPointer;
                    sv_RemotePointerCanvas.Items.Add(pointerContainer);
                    int X = (int)(addedPointer.X * canvasesContainer.Width);
                    int Y = (int)(addedPointer.Y * canvasesContainer.Height);
                    pointerContainer.Center = new System.Windows.Point(X, Y);
                    sv_RemotePointerCanvas.UpdateLayout();

                    Utilities.BrainstormingEventLogger.GetInstance(dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_RemotePointerAdded(addedPointer));
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
                    ScatterViewItem pointerContainer = findViewItemWithPointerID(updatedPointer.Id);
                    if (pointerContainer == null)
                    {
                        return;
                    }
                    if (!updatedPointer.IsActive)
                    {
                        //make pointer fade out
                        DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(2));
                        pointerContainer.BeginAnimation(UserControl.OpacityProperty, anim);
                        //pointerContainer.Visibility = System.Windows.Visibility.Hidden;
                        sv_RemotePointerCanvas.UpdateLayout();
                        pointerContainer.Tag = updatedPointer;
                        Utilities.BrainstormingEventLogger.GetInstance(dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_RemotePointerLeft(updatedPointer));
                    }
                    else
                    {
                        //make pointer fade in
                        if (!((RemotePointer)pointerContainer.Tag).IsActive)
                        {
                            DoubleAnimation anim = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2));
                            pointerContainer.BeginAnimation(UserControl.OpacityProperty, anim);
                            Utilities.BrainstormingEventLogger.GetInstance(dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_RemotePointerReentered(updatedPointer));
                        }
                        //update location
                        int X = (int)(updatedPointer.X * canvasesContainer.Width);
                        int Y = (int)(updatedPointer.Y * canvasesContainer.Height);
                        pointerContainer.Center = new System.Windows.Point(X, Y);
                        if (((RemotePointer)pointerContainer.Tag).IsActive)
                        {
                            Utilities.BrainstormingEventLogger.GetInstance(dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_RemotePointerMoved(updatedPointer));
                        }
                        pointerContainer.Tag = updatedPointer;
                        sv_RemotePointerCanvas.UpdateLayout();
                    }
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }));
        }
        ScatterViewItem findViewItemWithPointerID(int pointerID)
        {
            ScatterViewItem matchedItem = null;
            foreach (ScatterViewItem item in sv_RemotePointerCanvas.Items)
            {
                RemotePointerUI remotePointer = (RemotePointerUI)(item.Content);
                if (remotePointer.PointerID == pointerID)
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

        private static object sync = new object();
        private AnotoNoteUpdater anotoNotesDownloader = null;
        private PostItGeneralManager brainstormManager;
        private CloudDataEventProcessor cloudDataEventProcessor = null;

        private NoteUpdater dropboxGeneralNoteDownloader = null;


        private AsyncTCPClient p2pClient = null;
        private RemotePointerManager remotePointerManager = null;
        //Network data processors
        private NoteUpdateScheduler noteUpdateScheduler = null;

        //PostItNetworkDataManager networkDataManager = null;
        //Timeline processors
        private TimelineControllers.TimelineChangeManager timelineManager;


        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            layoutMenu();
        }

        private Utilities.BrainstormingEventLogger brainstormLogger;
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
            Utilities.BrainstormingEventLogger.GetInstance(dropboxGeneralNoteDownloader.Storage).UploadLogString(Utilities.BrainstormingEventLogger.getLogStr_Start());
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Utilities.BrainstormingEventLogger.GetInstance(dropboxGeneralNoteDownloader.Storage).Close();
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
                noteUpdateScheduler.Stop();
                dropboxGeneralNoteDownloader.Close();
                anotoNotesDownloader.Close();

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