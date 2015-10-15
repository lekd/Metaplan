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
        WIFIConnectionManager wifiManager = null;
        //Network data processors
        NoteUpdateScheduler noteUpdateScheduler = null;
        DropboxNoteDownloader dropboxDownloader = null;
        CloudDataEventProcessor cloudDataEventProcessor = null;

        PostItNetworkDataManager networkDataManager = null;

        //Children PostIt UI Collection
        Dictionary<int, IPostItUI> childrenPostItNotes = new Dictionary<int, IPostItUI>();
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
        }
        void InitNetworkCommManager()
        {
            networkDataManager = new PostItNetworkDataManager();
            networkDataManager.GenericPostItDecoder.commandDecodedEventHandler +=new GenericPostItCommandDecoder.PostItCommandDecodedEvent(brainstormManager.GenericNoteManager.ProcessPostItNoteCommand);

            wifiManager = new WIFIConnectionManager();
            wifiManager.clientConnectedHandler += new WIFIConnectionManager.ClientConnectedEvent(wifiManager_clientConnectedHandler);
            wifiManager.dataReceivedHandler += new WIFIConnectionManager.DataReceivedEvent(networkDataManager.networkReceivedDataHandler);
            wifiManager.start();

            //processors related to cloud service
            dropboxDownloader = new DropboxNoteDownloader();
            noteUpdateScheduler = new NoteUpdateScheduler();
            noteUpdateScheduler.updateEventHandler += new NoteUpdateScheduler.UpdateIntervalTickedEvent(noteUpdateScheduler_updateEventHandler);
            cloudDataEventProcessor = new CloudDataEventProcessor();
            dropboxDownloader.noteStreamsDownloadedHandler += new DropboxNoteDownloader.NewNoteStreamsDownloaded(cloudDataEventProcessor.handleDownloadedStreamsFromCloud);
            cloudDataEventProcessor.newNoteExtractedEventHandler += new CloudDataEventProcessor.NewNoteExtractedFromStreamEvent(brainstormManager.GenericNoteManager.addNote);
        }

        void noteUpdateScheduler_updateEventHandler()
        {
            Thread dropboxUpdateThread = new Thread(new ThreadStart(dropboxDownloader.UpdateNotes));
            dropboxUpdateThread.Start();
        }
        void InitBrainstormingProcessors()
        {
            brainstormManager = new PostItGeneralManager();
            brainstormManager.ideaAddedEventHandler += new PostItGeneralManager.NewIdeaAddedEvent(brainstormManager_ideaAddedEventHandler);
            brainstormManager.ideaUpdatedHandler += new PostItGeneralManager.IdeaUpdatedEvent(brainstormManager_ideaUpdatedHandler);
            brainstormManager.ideaRemovedHandler += new PostItGeneralManager.IdeaRemovedEvent(brainstormManager_ideaRemovedHandler);
        }
        object sync = new object();
        void brainstormManager_ideaAddedEventHandler(GenericIdeationObjects.IdeationUnit addedIdea)
        {
            lock (sync)
            {
                string msg = "Note " + addedIdea.Id.ToString() +
                " added at " + addedIdea.Left.ToString() + "," + addedIdea.Top.ToString();
                addMessageToListBox(msg);
                addNewIdeaUI(addedIdea);
            }
            
        }

        void brainstormManager_ideaUpdatedHandler(GenericIdeationObjects.IdeationUnit updatedIdea)
        {
            lock (sync)
            {
                string msg = "Note " + updatedIdea.Id.ToString() + " updated";
                addMessageToListBox(msg);
                this.Dispatcher.BeginInvoke(new Action<IdeationUnit>((ideaToUpdate) =>
                {
                    IPostItUI tobeUpdated = childrenPostItNotes[ideaToUpdate.Id];
                    tobeUpdated.updateDisplayedContent(ideaToUpdate.Content);
                    if (tobeUpdated is AnotoPostItUI)
                    {
                        return;
                    }
                    ScatterViewItem container = null;
                    foreach (ScatterViewItem item in sv_MainCanvas.Items)
                    {
                        if (item.Content == tobeUpdated)
                        {
                            container = item;
                        }
                    }
                    if (container != null)
                    {
                        PointF globalTopLeft = Utilities.UtilitiesLib.convertRelativeCoordinateToGlobalCoordinate(ideaToUpdate.Left, ideaToUpdate.Top, (float)sv_MainCanvas.Width, (float)sv_MainCanvas.Height);
                        double newCenterX = (float)(globalTopLeft.X + container.Width/2);
                        double newCenterY = (float)(globalTopLeft.Y + container.Height / 2);
                        container.Center = new System.Windows.Point((int)newCenterX, (int)newCenterY);
                    }
                }), new object[] { updatedIdea });
            }
            
            
        }

        void brainstormManager_ideaRemovedHandler(GenericIdeationObjects.IdeationUnit removedIdea)
        {
            lock (sync)
            {
                string msg = "Note " + removedIdea.Id.ToString() + " removed";
                addMessageToListBox(msg);
                this.Dispatcher.BeginInvoke(new Action<IdeationUnit>((ideaToRemove) =>
                {
                    IPostItUI tobeRemoved = childrenPostItNotes[ideaToRemove.Id];
                    for (int i = 0; i < sv_MainCanvas.Items.Count; i++)
                    {
                        if (((ScatterViewItem)sv_MainCanvas.Items[i]).Content == tobeRemoved)
                        {
                            sv_MainCanvas.Items.RemoveAt(i);
                            childrenPostItNotes.Remove(ideaToRemove.Id);
                            break;
                        }
                    }
                    TakeASnapshot();
                }), new object[] { removedIdea });
            }
            
        }

        void wifiManager_clientConnectedHandler(WifiDataReceiver receiver)
        {
            addMessageToListBox(receiver.Commsocket.RemoteEndPoint.ToString());
        }
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
        void addNewIdeaUI(IdeationUnit idea)
        {

            if (childrenPostItNotes.ContainsKey(idea.Id))
            {
                return;
            }
            this.Dispatcher.BeginInvoke(new Action<IdeationUnit>((ideaToAdd) =>
            {
                if (ideaToAdd is AnotoPostIt)
                {
                    AnotoPostItUI noteUI = new AnotoPostItUI();
                    noteUI.setNoteID(ideaToAdd.Id);
                    noteUI.updateDisplayedContent(ideaToAdd.Content);

                    childrenPostItNotes.Add(noteUI.getNoteID(), noteUI);

                    ScatterViewItem container = new ScatterViewItem();
                    container.Content = noteUI;
                    container.Width = noteUI.InitWidth;
                    container.Height = noteUI.InitHeight;
                    container.Orientation = 0;
                    int centerX = (int)(ideaToAdd.Left + container.Width/2);
                    int centerY = (int)(ideaToAdd.Top + container.Height/2);
                    container.Center = new System.Windows.Point(centerX, centerY);
                    sv_MainCanvas.Items.Add(container);
                }
                else if (ideaToAdd is PostItNote)
                {
                    PostItNote castNote = (PostItNote)ideaToAdd;
                    if (castNote.DataType == PostItContentDataType.Photo
                        || castNote.DataType == PostItContentDataType.WritingImage)
                    {
                        
                        ImageBasedPostItUI noteUI = new ImageBasedPostItUI();
                        noteUI.setNoteID(castNote.Id);
                        noteUI.updateDisplayedContent(castNote.Content);

                        childrenPostItNotes.Add(noteUI.getNoteID(), noteUI);

                        ScatterViewItem container = new ScatterViewItem();
                        container.Content = noteUI;
                        container.Width = noteUI.InitWidth;
                        container.Height = noteUI.InitHeight;
                        float left = castNote.Left == 0 ? 0.5f : castNote.Left;
                        float top = castNote.Top == 0 ? 0.5f : castNote.Left;
                        PointF globalTopLeft = Utilities.UtilitiesLib.convertRelativeCoordinateToGlobalCoordinate(left, top, (float)sv_MainCanvas.Width, (float)sv_MainCanvas.Height);
                        int centerX = (int)(globalTopLeft.X + container.Width / 2);
                        int centerY = (int)(globalTopLeft.Y + container.Height / 2);
                        container.Center = new System.Windows.Point(centerX, centerY);
                        sv_MainCanvas.Items.Add(container);
                    }
                }
                TakeASnapshot();
            }), new object[] { idea});
        }



        private void sv_MainCanvas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point doubleClickPos = e.GetPosition(sv_MainCanvas);
            TextPostItUI noteUI = new TextPostItUI();
            ScatterViewItem svItem = new ScatterViewItem();
            svItem.Content = noteUI;
            svItem.Width = 200;
            svItem.Height = 200;
            svItem.Center = doubleClickPos;
            sv_MainCanvas.Items.Add(svItem);
            noteUI._removalEventHandler += new TextPostItUI.NoteRemovalEventHandler(noteUI__removalEventHandler);
            noteUI._updateEventHandler += new TextPostItUI.NoteUpdateEventHandler(noteUI__updateEventHandler);

            PostItObjects.TextPostIt postitNote = new PostItObjects.TextPostIt();
            noteUI.PostIt = postitNote;
            brainstormManager.AddIdea(postitNote);
        }

        void noteUI__updateEventHandler(PostItObjects.TextPostIt tobeupdated_note)
        {
            
            
        }

        void noteUI__removalEventHandler(object sender, PostItObjects.TextPostIt toberemoved_note)
        {
            TextPostItUI noteUI = (TextPostItUI)sender;
            foreach (ScatterViewItem sv_item in sv_MainCanvas.Items)
            {
                if (sv_item.Content == noteUI)
                {
                    sv_MainCanvas.Items.Remove(sv_item);
                    
                    brainstormManager.RemoveIdea(noteUI.PostIt);
                    break;
                }
            }
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
             * */
        }
        void TakeASnapshot()
        {
            sv_MainCanvas.UpdateLayout();
            double dpi = 96;
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)sv_MainCanvas.Width, (int)sv_MainCanvas.Height, dpi, dpi, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(sv_MainCanvas);
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (Stream fileStream = File.Create("Snapshot.png"))
            {
                pngImage.Save(fileStream);
            }
        }
    }
}