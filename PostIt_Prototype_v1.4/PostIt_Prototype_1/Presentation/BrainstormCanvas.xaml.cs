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

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for BrainstormCanvas.xaml
    /// </summary>
    public partial class BrainstormCanvas : SurfaceWindow
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
            InitMenu();
            InitBrainstormingProcessors();
            InitNetworkCommManager();
            InitTimeline();

            DrawingCanvasModeSwitcher.normalDrawingAttribute = drawingCanvas.DefaultDrawingAttributes.Clone();

        }


        #endregion Public Constructors

        #region Protected Methods

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

        #endregion Protected Methods

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

        private void addNewIdeaUIs(List<IdeationUnit> ideas, bool asInit)
        {
            Random rnd = new Random();
            this.Dispatcher.Invoke(new Action<List<IdeationUnit>, bool>((ideasToAdd, init) =>
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

                    container.Content = noteUI;
                    container.Width = noteUI.InitWidth;
                    container.Height = noteUI.InitHeight;

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
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
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
            sv_MainCanvas.Width = this.Width;
            sv_MainCanvas.Height = this.Height;
            drawingCanvas.Width = this.Width;
            drawingCanvas.Height = this.Height;
            canvasesContainer.Width = this.Width;
            canvasesContainer.Height = this.Height;
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

        private void InitBrainstormingProcessors()
        {
            brainstormManager = new PostItGeneralManager();
            brainstormManager.ideaAddedEventHandler += new PostItGeneralManager.NewIdeaAddedEvent(brainstormManager_ideaAddedEventHandler);
            brainstormManager.ideaUpdatedHandler += new PostItGeneralManager.IdeaUpdatedEvent(brainstormManager_ideaUpdatedHandler);
            brainstormManager.ideaRemovedHandler += new PostItGeneralManager.IdeaRemovedEvent(brainstormManager_ideaRemovedHandler);
            brainstormManager.ideaRestoredHandler += new PostItGeneralManager.IdeaRestoredEvent(brainstormManager_ideaRestoredHandler);
            brainstormManager.ideaCollectionRollBackFinishedEventHandler += new PostItGeneralManager.IdeaCollectionRollBackFinished(brainstormManager_ideaCollectionRollBackFinishedEventHandler);

            brainstormManager.TrashManager.discardedIdeaReceivedEventHandler += new Recycle_Bin.RecycleBinManager.DiscardedIdeaReceived(recycleBin.AddDiscardedIdea);

            recycleBin.noteRestoredEventHandler += new Recycle_Bin.RecycleBinManager.DiscardedIdeaRestored(brainstormManager.TrashManager.RestoreIdea);
        }

        private void InitMenu()
        {
            //MainMenu.Radius = this.Height * 0.15;
            Thickness mainMenuMargin = MainMenu.Margin;
            mainMenuMargin.Left = this.Width / 2 - MainMenu.Radius;
            mainMenuMargin.Top = this.Height - 2.25 * MainMenu.Radius;
            MainMenu.Margin = mainMenuMargin;
        }

        private void InitNetworkCommManager()
        {
            /*networkDataManager = new PostItNetworkDataManager();
            networkDataManager.GenericPostItDecoder.commandDecodedEventHandler +=new GenericPostItCommandDecoder.PostItCommandDecodedEvent(brainstormManager.GenericNoteManager.ProcessPostItNoteCommand);

            wifiManager = new WIFIConnectionManager();
            wifiManager.clientConnectedHandler += new WIFIConnectionManager.ClientConnectedEvent(wifiManager_clientConnectedHandler);
            wifiManager.dataReceivedHandler += new WIFIConnectionManager.DataReceivedEvent(networkDataManager.networkReceivedDataHandler);
            wifiManager.start();*/

            //processors related to cloud service
            dropboxGeneralNoteDownloader = new DropboxNoteUpDownloader();
            anotoNotesDownloader = new AnotoNotesDownloader();
            noteUpdateScheduler = new NoteUpdateScheduler();
            noteUpdateScheduler.updateEventHandler += new NoteUpdateScheduler.UpdateIntervalTickedEvent(noteUpdateScheduler_updateEventHandler);
            cloudDataEventProcessor = new CloudDataEventProcessor();
            dropboxGeneralNoteDownloader.noteStreamsDownloadedHandler += new NewNoteStreamsDownloaded(cloudDataEventProcessor.handleDownloadedStreamsFromCloud);
            anotoNotesDownloader.noteStreamsDownloadedHandler += new NewNoteStreamsDownloaded(cloudDataEventProcessor.handleDownloadedStreamsFromCloud);
            cloudDataEventProcessor.newNoteExtractedEventHandler += new CloudDataEventProcessor.NewNoteExtractedFromStreamEvent(brainstormManager.HandleComingIdea);
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
            timelineManager.EventIntepreter = eventIntepreter;
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

        private void noteUIDeletedEventHandler(object sender, IdeationUnit associatedIdea)
        {
            brainstormManager.RemoveIdea(associatedIdea);
        }

        private void noteUIManipluatedEventHandler(object sender, IdeationUnit underlyingIdea, float newX, float newY)
        {
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

        private void refreshBrainstormingWhiteboard()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                sv_MainCanvas.UpdateLayout();
            }));
        }

        private void removeNoteUI(IdeationUnit associatedIdea)
        {
            this.Dispatcher.Invoke(new Action<IdeationUnit>((ideaToAdd) =>
            {
                try
                {
                    ScatterViewItem ideaContainer = findNoteContainerOfIdea(ideaToAdd);
                    sv_MainCanvas.Items.Remove(ideaContainer);
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }), new object[] { associatedIdea });
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
                    RenderTargetBitmap noteContainerRenderTargetBitmap = new RenderTargetBitmap((int)canvasesContainer.Width, (int)canvasesContainer.Height, dpi, dpi, PixelFormats.Pbgra32);
                    noteContainerRenderTargetBitmap.Render(canvasesContainer);
                    ImageSource NoteContainerImgSrc = (ImageSource)noteContainerRenderTargetBitmap.Clone();
                    BitmapFrame resizedNoteContainerBmpFrame = Utilities.UtilitiesLib.CreateResizedBitmapFrame(NoteContainerImgSrc, (int)(canvasesContainer.Width * 3 / 4), (int)(canvasesContainer.Height * 3 / 4), 0);
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
                        Task.Factory.StartNew(() => { 
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

        private void timelineView_frameSelectedEventHandler(int selectedFrameID)
        {
            timelineManager.SelectFrame(selectedFrameID);
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
                    }
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }), new object[] { updatedIdea });
        }

        #endregion Private Methods

        #region Private Fields

        private static object sync = new object();
        private AnotoNotesDownloader anotoNotesDownloader = null;
        private PostItGeneralManager brainstormManager;
        private CloudDataEventProcessor cloudDataEventProcessor = null;

        private DropboxNoteUpDownloader dropboxGeneralNoteDownloader = null;

        //List<AnotoInkManager> inkManagers = new List<AnotoInkManager>();
        //WIFIConnectionManager wifiManager = null;
        //Network data processors
        private NoteUpdateScheduler noteUpdateScheduler = null;

        //PostItNetworkDataManager networkDataManager = null;
        //Timeline processors
        private TimelineControllers.TimelineChangeManager timelineManager;

        #endregion Private Fields

        /* Runs on UI thread */
    }
}