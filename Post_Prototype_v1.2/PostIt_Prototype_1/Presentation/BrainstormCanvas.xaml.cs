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
        DropboxNoteUpDownloader dropboxDownloader = null;
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
            InitBrainstormingProcessors();
            InitNetworkCommManager();
            InitTimeline();
        }
        void InitNetworkCommManager()
        {
            /*networkDataManager = new PostItNetworkDataManager();
            networkDataManager.GenericPostItDecoder.commandDecodedEventHandler +=new GenericPostItCommandDecoder.PostItCommandDecodedEvent(brainstormManager.GenericNoteManager.ProcessPostItNoteCommand);

            wifiManager = new WIFIConnectionManager();
            wifiManager.clientConnectedHandler += new WIFIConnectionManager.ClientConnectedEvent(wifiManager_clientConnectedHandler);
            wifiManager.dataReceivedHandler += new WIFIConnectionManager.DataReceivedEvent(networkDataManager.networkReceivedDataHandler);
            wifiManager.start();*/

            //processors related to cloud service
            dropboxDownloader = new DropboxNoteUpDownloader();
            noteUpdateScheduler = new NoteUpdateScheduler();
            noteUpdateScheduler.updateEventHandler += new NoteUpdateScheduler.UpdateIntervalTickedEvent(noteUpdateScheduler_updateEventHandler);
            cloudDataEventProcessor = new CloudDataEventProcessor();
            dropboxDownloader.noteStreamsDownloadedHandler += new DropboxNoteUpDownloader.NewNoteStreamsDownloaded(cloudDataEventProcessor.handleDownloadedStreamsFromCloud);
            cloudDataEventProcessor.newNoteExtractedEventHandler += new CloudDataEventProcessor.NewNoteExtractedFromStreamEvent(brainstormManager.HandleComingIdea);
        }
        #region timeline initialization
        void InitTimeline()
        {
            timelineView.frameSelectedEventHandler += new BrainstormingTimelineUI.TimelineFrameSelected(timelineView_frameSelectedEventHandler);
            timelineManager = new TimelineControllers.TimelineChangeManager();
            timelineManager.newTimelineFrameAddedEventHandler += new TimelineControllers.TimelineChangeManager.NewTimelineFrameAdded(newTimelineFrameAddedEventHandler);
            timelineManager.startEnumeratingEventHandler +=new TimelineControllers.TimelineChangeManager.StartEnumeratingFromBeginning(brainstormManager.reset);
            timelineManager.finishEnumeratingEventHandler +=new TimelineControllers.TimelineChangeManager.FinishEnumeratingToTheSelected(brainstormManager.notifyIdeaCollectionRollBack);

            TimelineControllers.TimlineEventIntepreter eventIntepreter = new TimelineControllers.TimlineEventIntepreter();
            eventIntepreter.ADDeventExtractedHandler += new TimelineControllers.TimlineEventIntepreter.ADDIdeaCommandExtracted(brainstormManager.AddIdeaInBackground);
            eventIntepreter.REMOVEeventExtractedHandler +=new TimelineControllers.TimlineEventIntepreter.REMOVEIdeaCommandExtracted(brainstormManager.RemoveIdeaInBackground);
            eventIntepreter.UPDATEPosEventExtractedHandler +=new TimelineControllers.TimlineEventIntepreter.UPDATEIdeaPositionCommandExtracted(brainstormManager.UpdateIdeaPositionInBackground);
            eventIntepreter.UPDATEContentEventExtractedHandler +=new TimelineControllers.TimlineEventIntepreter.UPDATEIdeaContentCommandExtracted(brainstormManager.UpdateIdeaContentInBackground);
            timelineManager.EventIntepreter = eventIntepreter;


        }

        void timelineView_frameSelectedEventHandler(int selectedFrameID)
        {
            timelineManager.SelectFrame(selectedFrameID);
        }

        void newTimelineFrameAddedEventHandler(TimelineControllers.TimelineFrame addedFrame)
        {
            this.Dispatcher.BeginInvoke(new Action<TimelineControllers.TimelineFrame>((frameToAdd) =>
            {
                timelineView.AddFrame(frameToAdd);
            }), new TimelineControllers.TimelineFrame[] { addedFrame });
        }
        #endregion
        void noteUpdateScheduler_updateEventHandler()
        {
            Thread dropboxUpdateThread = new Thread(new ThreadStart(dropboxDownloader.UpdateNotes));
            dropboxUpdateThread.Start();
        }
        #region brainstorming manager
        void InitBrainstormingProcessors()
        {
            brainstormManager = new PostItGeneralManager();
            brainstormManager.ideaAddedEventHandler += new PostItGeneralManager.NewIdeaAddedEvent(brainstormManager_ideaAddedEventHandler);
            brainstormManager.ideaUpdatedHandler += new PostItGeneralManager.IdeaUpdatedEvent(brainstormManager_ideaUpdatedHandler);
            brainstormManager.ideaRemovedHandler += new PostItGeneralManager.IdeaRemovedEvent(brainstormManager_ideaRemovedHandler);
            brainstormManager.ideaCollectionRollBackFinishedEventHandler += new PostItGeneralManager.IdeaCollectionRollBackFinished(brainstormManager_ideaCollectionRollBackFinishedEventHandler);
        }
        object sync = new object();
        void brainstormManager_ideaCollectionRollBackFinishedEventHandler(List<IdeationUnit> currentIdeas)
        {
            //clearNotes();
            this.Dispatcher.BeginInvoke(new Action<List<IdeationUnit>>((ideasToAdd) =>
            {
                sv_MainCanvas.Items.Clear();
                Random rnd = new Random();
                foreach (IdeationUnit idea in ideasToAdd)
                {
                    if(!idea.IsAvailable)
                    {
                        continue;
                    }
                    IPostItUI addedIdeaUI = null;
                    ScatterViewItem container = new ScatterViewItem();
                    if (idea is PostItNote)
                    {
                        PostItNote castNote = (PostItNote)idea;
                        if (castNote.Content is Bitmap)
                        {

                            ImageBasedPostItUI noteUI = new ImageBasedPostItUI();
                            noteUI.Tag = idea;
                            noteUI.setNoteID(castNote.Id);
                            noteUI.updateDisplayedContent(((Bitmap)castNote.Content).Clone());

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
                            container.Orientation = rnd.Next(-40, 40);
                            addedIdeaUI = noteUI;
                            addedIdeaUI.InitContainer(container);
                        }
                    }
                    if (addedIdeaUI != null)
                    {
                        addedIdeaUI.noteUITranslatedEventHandler += new NoteUITranslatedEvent(noteUIManipluatedEventHandler);
                        addedIdeaUI.noteUIDeletedEventHandler += new NoteUIDeletedEvent(noteUIDeletedEventHandler);
                    }
                }
                sv_MainCanvas.UpdateLayout();
            }), new object[] { currentIdeas });
            
        }
        void brainstormManager_ideaAddedEventHandler(GenericIdeationObjects.IdeationUnit addedIdea)
        {
            lock (sync)
            {
                string msg = "Note " + addedIdea.Id.ToString() +
                " added at " + addedIdea.CenterX.ToString() + "," + addedIdea.CenterY.ToString();
                
                addMessageToListBox(msg);
                addNewIdeaUI(addedIdea,true);
                TakeASnapshot();
                timelineManager.AddADDChange(addedIdea);
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
        #endregion
        void BrainstormCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            sv_MainCanvas.Width = this.Width;
            sv_MainCanvas.Height = this.Height;
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
            dropboxDownloader.Close();
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
        void addNewIdeaUI(IdeationUnit idea,bool asInit)
        {
            this.Dispatcher.BeginInvoke(new Action<IdeationUnit>((ideaToAdd) =>
            {
                IPostItUI addedIdeaUI = null;
                ScatterViewItem container = new ScatterViewItem();
                if (ideaToAdd is PostItNote)
                {
                    PostItNote castNote = (PostItNote)ideaToAdd;
                    if (castNote.Content is Bitmap)
                    {
                        
                        ImageBasedPostItUI noteUI = new ImageBasedPostItUI();
                        noteUI.Tag = ideaToAdd;
                        noteUI.setNoteID(castNote.Id);
                        noteUI.updateDisplayedContent(((Bitmap)castNote.Content).Clone());

                        container.Content = noteUI;
                        container.Width = noteUI.InitWidth;
                        container.Height = noteUI.InitHeight;

                        if (ideaToAdd.CenterX == 0 && ideaToAdd.CenterY == 0)
                        {
                            int centerX = (int)(container.Width / 2);
                            int centerY = (int)(sv_MainCanvas.Height - container.Height / 2);
                            ideaToAdd.CenterX = centerX;
                            ideaToAdd.CenterY = centerY;
                        }
                        sv_MainCanvas.Items.Add(container);
                        container.Center = new System.Windows.Point(ideaToAdd.CenterX, ideaToAdd.CenterY);
                        container.Orientation = (new Random()).Next(-40, 40);
                        addedIdeaUI = noteUI;
                        addedIdeaUI.InitContainer(container);
                    }
                }
                if (addedIdeaUI != null)
                {
                    if (asInit)
                    {
                        addedIdeaUI.startJustAddedAnimation(container.Orientation);
                    }
                    addedIdeaUI.noteUITranslatedEventHandler += new NoteUITranslatedEvent(noteUIManipluatedEventHandler);
                    addedIdeaUI.noteUIDeletedEventHandler += new NoteUIDeletedEvent(noteUIDeletedEventHandler);
                }
            }), new object[] { idea});
        }

        void noteUIManipluatedEventHandler(object sender, IdeationUnit underlyingIdea,float newX, float newY)
        {
            brainstormManager.UpdateIdeaPosition(underlyingIdea.Id, newX, newY);
        }
        void noteUIDeletedEventHandler(object sender, IdeationUnit associatedIdea)
        {
            brainstormManager.RemoveIdea(associatedIdea);
        }

        void addMessageToListBox(string message)
        {
            /*
            this.Dispatcher.BeginInvoke(new Action<string>((messageToAdd) =>
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = messageToAdd;
                lb_messages.Items.Add(item);
            }), new object[] { message });
            */
        }
        #region user-defined methods
        void TakeASnapshot()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                sv_MainCanvas.UpdateLayout();
                double dpi = 96;
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)sv_MainCanvas.Width, (int)sv_MainCanvas.Height, dpi, dpi, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(sv_MainCanvas);
                ImageSource imgSrc = (ImageSource)renderTargetBitmap.Clone();
                BitmapFrame resizedBmpFrame = Utilities.UtilitiesLib.CreateResizedBitmapFrame(imgSrc, (int)(sv_MainCanvas.Width *3 / 4), (int)(sv_MainCanvas.Height*3 / 4), 0);
                PngBitmapEncoder imageEncoder = new PngBitmapEncoder();
                imageEncoder.Frames.Add(BitmapFrame.Create(resizedBmpFrame));
                //imageEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                byte[] screenshotBytes = new byte[1];
                using (MemoryStream stream = new MemoryStream())
                {
                    imageEncoder.Save(stream);
                    stream.Seek(0, 0);
                    screenshotBytes = stream.ToArray();
                    Utilities.GlobalObjects.currentScreenshotBytes = screenshotBytes;
                }
                Thread uploadThread = new Thread(() => dropboxDownloader.UpdateMetaplanBoardScreen(screenshotBytes));
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
        void removeNoteUI(IdeationUnit associatedIdea)
        {
            this.Dispatcher.BeginInvoke(new Action<IdeationUnit>((ideaToAdd) =>
            {
                ScatterViewItem ideaContainer = findNoteContainerOfIdea(ideaToAdd);
                sv_MainCanvas.Items.Remove(ideaContainer);
                //TakeASnapshot();
                //timelineManager.AddDELETEChange(ideaToAdd.Id);
            }), new object[] { associatedIdea });
        }
        void updateNoteUIContent(GenericIdeationObjects.IdeationUnit updatedIdea)
        {
            this.Dispatcher.BeginInvoke(new Action<IdeationUnit>((ideaToUpdate) =>
            {
                ScatterViewItem noteContainer = findNoteContainerOfIdea(ideaToUpdate);
                IPostItUI noteUI = (IPostItUI)noteContainer.Content;
                noteUI.update(ideaToUpdate);
                noteContainer.Content = noteUI;
            }), new object[] { updatedIdea });
        }
        void updateNoteUIPosition(GenericIdeationObjects.IdeationUnit updatedIdea)
        {
            this.Dispatcher.BeginInvoke(new Action<IdeationUnit>((ideaToUpdate) =>
            {
                ScatterViewItem noteContainer = findNoteContainerOfIdea(ideaToUpdate);
                noteContainer.Center = new System.Windows.Point(ideaToUpdate.CenterX, ideaToUpdate.CenterY);
            }), new object[] { updatedIdea });
        }
        void clearNotes()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                sv_MainCanvas.Items.Clear();
                sv_MainCanvas.UpdateLayout();
            }));
        }
        void refreshBrainstormingWhiteboard()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                sv_MainCanvas.UpdateLayout();
                MessageBox.Show("Number of notes " + sv_MainCanvas.Items.Count.ToString());
            }));
        }
        #endregion
    }
}