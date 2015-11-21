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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using PostIt_Prototype_1.PostItBrainstorming;
using PostIt_Prototype_1.NetworkCommunicator;
using PostIt_Prototype_1.PostItDataHandlers;
using PostIt_Prototype_1.PostItObjects;
using GenericIdeationObjects;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Ink;
using System.Diagnostics;

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for BrainstormCanvas.xaml
    /// </summary>
    public partial class BrainstormCanvas : SurfaceWindow
    {
        PostItGeneralManager brainstormManager;
        //List<AnotoInkManager> inkManagers = new List<AnotoInkManager>();
        //WIFIConnectionManager wifiManager = null;
        //Network data processors
        NoteUpdateScheduler noteUpdateScheduler = null;
        DropboxNoteUpDownloader dropboxGeneralNoteDownloader = null;
        AnotoNotesDownloader anotoNotesDownloader = null;
        CloudDataEventProcessor cloudDataEventProcessor = null;

        //PostItNetworkDataManager networkDataManager = null;
        //Timeline processors
        TimelineControllers.TimelineChangeManager timelineManager;


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
        }
        #region MenuUI
        void InitMenu()
        {
            //MainMenu.Radius = this.Height * 0.15;
            Thickness mainMenuMargin = MainMenu.Margin;
            mainMenuMargin.Left = this.Width / 2 - MainMenu.Radius;
            mainMenuMargin.Top = this.Height - 2.25 * MainMenu.Radius;
            MainMenu.Margin = mainMenuMargin;
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
        #endregion
        void InitNetworkCommManager()
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
            dropboxGeneralNoteDownloader.noteStreamsDownloadedHandler += new DropboxNoteUpDownloader.NewNoteStreamsDownloaded(cloudDataEventProcessor.handleDownloadedStreamsFromCloud);
            anotoNotesDownloader.noteStreamsDownloadedHandler += new DropboxNoteUpDownloader.NewNoteStreamsDownloaded(cloudDataEventProcessor.handleDownloadedStreamsFromCloud);
            cloudDataEventProcessor.newNoteExtractedEventHandler += new CloudDataEventProcessor.NewNoteExtractedFromStreamEvent(brainstormManager.HandleComingIdea);
        }
        #region timeline initialization
        void InitTimeline()
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

        void timelineView_frameSelectedEventHandler(int selectedFrameID)
        {
            timelineManager.SelectFrame(selectedFrameID);
        }

        void newTimelineFrameAddedEventHandler(TimelineControllers.TimelineFrame addedFrame)
        {
            this.Dispatcher.Invoke(new Action<TimelineControllers.TimelineFrame>((frameToAdd) =>
            {
                timelineView.AddFrame(frameToAdd);
            }), new TimelineControllers.TimelineFrame[] { addedFrame });
        }
        #endregion
        void noteUpdateScheduler_updateEventHandler()
        {
            //Thread dropboxImageNoteUpdateThread = new Thread(new ThreadStart(dropboxGeneralNoteDownloader.UpdateNotes));
            //dropboxImageNoteUpdateThread.Start();
            dropboxGeneralNoteDownloader.UpdateNotes();

            //Thread anotoUpdateThread = new Thread(new ThreadStart(anotoNotesDownloader.UpdateNotes));
            //anotoUpdateThread.Start();
            anotoNotesDownloader.UpdateNotes();
        }
        #region brainstorming manager
        void InitBrainstormingProcessors()
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

        static object sync = new object();
        void brainstormManager_ideaCollectionRollBackFinishedEventHandler(List<IdeationUnit> currentIdeas)
        {
            clearNotes();
            lock (sync)
            {
                addNewIdeaUIs(currentIdeas, false);
                recycleBin.RefreshNewDiscardedIdeasList(currentIdeas);
            }
        }

        private int callcounter = 0;
        /* Runs on UI thread */
        void brainstormManager_ideaAddedEventHandler(GenericIdeationObjects.IdeationUnit addedIdea)
        {
            Debug.WriteLine("{0}", callcounter++);
            Thread.Sleep(1000);
            lock (sync)
            {
                List<IdeationUnit> oneItemList = new List<IdeationUnit>();
                oneItemList.Add(addedIdea);
                addNewIdeaUIs(oneItemList, true);
                TakeASnapshot();
                timelineManager.AddADDChange(addedIdea);
                //Thread.Sleep(100);
            }

        }
        void brainstormManager_ideaRemovedHandler(GenericIdeationObjects.IdeationUnit removedIdea)
        {
            lock (sync)
            {
                string msg = "Note " + removedIdea.Id.ToString() + " removed";
                //addMessageToListBox(msg);
                removeNoteUI(removedIdea);
                TakeASnapshot();
                timelineManager.AddDELETEChange(removedIdea.Id);
            }

        }
        void brainstormManager_ideaRestoredHandler(IdeationUnit restoredIdea)
        {
            lock (sync)
            {
                List<IdeationUnit> oneItemList = new List<IdeationUnit>();
                oneItemList.Add(restoredIdea);
                addNewIdeaUIs(oneItemList, false);
                TakeASnapshot();
                timelineManager.AddRESTOREChange(restoredIdea.Id);
            }
        }
        void brainstormManager_ideaUpdatedHandler(GenericIdeationObjects.IdeationUnit updatedIdea, GenericIdeationObjects.IdeationUnit.IdeaUpdateType updateType)
        {
            lock (sync)
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

        #endregion
        #region General UI Initialization
        void BrainstormCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            sv_MainCanvas.Width = this.Width;
            sv_MainCanvas.Height = this.Height;
            drawingCanvas.Width = this.Width;
            drawingCanvas.Height = this.Height;
            canvasesContainer.Width = this.Width;
            canvasesContainer.Height = this.Height;
            Loaded -= BrainstormCanvas_Loaded;
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
            noteUpdateScheduler.Stop();
            dropboxGeneralNoteDownloader.Close();
            anotoNotesDownloader.Close();
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
        #endregion

        #region Note UI events
        void noteUIManipluatedEventHandler(object sender, IdeationUnit underlyingIdea, float newX, float newY)
        {
            brainstormManager.UpdateIdeaPosition(underlyingIdea.Id, newX, newY);
        }
        void noteUIDeletedEventHandler(object sender, IdeationUnit associatedIdea)
        {
            brainstormManager.RemoveIdea(associatedIdea);
        }
        #endregion

        #region user-defined methods
        void TakeASnapshot()
        {
            this.Dispatcher.Invoke(new Action(() =>
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
                }
                Thread uploadThread = new Thread(() => dropboxGeneralNoteDownloader.UpdateMetaplanBoardScreen(screenshotBytes));
                uploadThread.Start();
            }));
        }
        ScatterViewItem findNoteContainerOfIdea(IdeationUnit idea)
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
        void addNewIdeaUIs(List<IdeationUnit> ideas, bool asInit)
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
        void AddSinglePostItNote(IdeationUnit idea, int initAngle, bool init)
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
        void AddSingleStrokeBasedNote(IdeationUnit strokeBasedIdea)
        {
            List<System.Windows.Point> strokePoints = (List<System.Windows.Point>)strokeBasedIdea.Content;
            StylusPointCollection stylusPoints = new StylusPointCollection();
            foreach (System.Windows.Point p in strokePoints)
            {
                StylusPoint stylusP = new StylusPoint(p.X, p.Y);
                stylusPoints.Add(stylusP);
            }
            Stroke newStroke = new Stroke(stylusPoints);
            newStroke.DrawingAttributes = drawingCanvas.DefaultDrawingAttributes;
            drawingCanvas.Strokes.Add(newStroke);
        }
        void AddSingleIdeaGroup(IdeationUnit ideaGroup)
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
        void removeNoteUI(IdeationUnit associatedIdea)
        {
            this.Dispatcher.Invoke(new Action<IdeationUnit>((ideaToAdd) =>
            {
                ScatterViewItem ideaContainer = findNoteContainerOfIdea(ideaToAdd);
                sv_MainCanvas.Items.Remove(ideaContainer);
                //TakeASnapshot();
                //timelineManager.AddDELETEChange(ideaToAdd.Id);
            }), new object[] { associatedIdea });
        }
        void updateNoteUIContent(GenericIdeationObjects.IdeationUnit updatedIdea)
        {
            this.Dispatcher.Invoke(new Action<IdeationUnit>((ideaToUpdate) =>
            {
                ScatterViewItem noteContainer = findNoteContainerOfIdea(ideaToUpdate);
                IPostItUI noteUI = (IPostItUI)noteContainer.Content;
                noteUI.update(ideaToUpdate);
                noteContainer.Content = noteUI;
            }), new object[] { updatedIdea.Clone() });
        }
        void updateNoteUIPosition(GenericIdeationObjects.IdeationUnit updatedIdea)
        {
            this.Dispatcher.Invoke(new Action<IdeationUnit>((ideaToUpdate) =>
            {
                ScatterViewItem noteContainer = findNoteContainerOfIdea(ideaToUpdate);
                if (noteContainer != null)
                {
                    noteContainer.Center = new System.Windows.Point(ideaToUpdate.CenterX, ideaToUpdate.CenterY);
                }
            }), new object[] { updatedIdea });
        }
        void clearNotes()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                sv_MainCanvas.Items.Clear();
                sv_MainCanvas.UpdateLayout();
                drawingCanvas.Strokes.Clear();
                drawingCanvas.UpdateLayout();
            }));
        }
        void refreshBrainstormingWhiteboard()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                sv_MainCanvas.UpdateLayout();
                MessageBox.Show("Number of notes " + sv_MainCanvas.Items.Count.ToString());
            }));
        }
        void addMessageToListBox(string message)
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
        #endregion

        #region drawing-related events
        private void drawingCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            sv_MainCanvas.IsHitTestVisible = true;
            Stroke latestStroke = e.Stroke;
            StylusPointCollection strokePoints = latestStroke.StylusPoints;
            if (strokePoints.Count < 10)
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
            IdeationUnit strokeIdea = new StrokeBasedIdea();
            strokeIdea.Id = IdeaIDGenerator.generateID();
            strokeIdea.Content = pathPoints;
            brainstormManager.AddIdeaInBackground(strokeIdea);
            TakeASnapshot();
            timelineManager.AddADDChange(strokeIdea);
        }

        private void sv_MainCanvas_TouchDown(object sender, TouchEventArgs e)
        {
            base.OnTouchDown(e);
            sv_MainCanvas.IsHitTestVisible = false;

        }

        private void sv_MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            sv_MainCanvas.IsHitTestVisible = false;

        }
        private void drawingCanvas_StrokeErasing(object sender, SurfaceInkCanvasStrokeErasingEventArgs e)
        {

        }
        private void drawingCanvas_StrokeErased(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}