using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Dropbox.Api.Files;
using GenericIdeationObjects;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using PostIt_Prototype_1.NetworkCommunicator;
using PostIt_Prototype_1.PostItBrainstorming;
using PostIt_Prototype_1.PostItDataHandlers;
using PostIt_Prototype_1.PostItObjects;
using PostIt_Prototype_1.Properties;
using PostIt_Prototype_1.TimelineControllers;
using PostIt_Prototype_1.Utilities;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Control = System.Windows.Controls.Control;
using File = Google.Apis.Drive.v3.Data.File;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Windows.Point;

namespace PostIt_Prototype_1.Presentation
{
    using File = Metadata;
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
            _backendStorage = Session.Storage;
            InitializeComponent();
            Loaded += BrainstormCanvas_Loaded;
        }

        #endregion Public Constructors

        #region Public Methods

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
                pointerContainer.Center = new Point(x, y);
                sv_RemotePointerCanvas.UpdateLayout();

                (await _eventLogger).UploadLogString(BrainstormingEventLogger.getLogStr_RemotePointerAdded(addedPointer));
            }
            catch (Exception ex)
            {
                UtilitiesLib.LogError(ex);
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
                    pointerContainer.BeginAnimation(OpacityProperty, anim);
                    //pointerContainer.Visibility = System.Windows.Visibility.Hidden;
                    sv_RemotePointerCanvas.UpdateLayout();
                    pointerContainer.Tag = updatedPointer;
                    (await _eventLogger).UploadLogString(BrainstormingEventLogger.getLogStr_RemotePointerLeft(updatedPointer));
                }
                else
                {
                    //make pointer fade in
                    if (!((RemotePointer)pointerContainer.Tag).IsActive)
                    {
                        var anim = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2));
                        pointerContainer.BeginAnimation(OpacityProperty, anim);
                        (await _eventLogger).UploadLogString(BrainstormingEventLogger.getLogStr_RemotePointerReentered(updatedPointer));
                    }
                    //update location
                    var x = (int)(updatedPointer.X * canvasesContainer.Width);
                    var y = (int)(updatedPointer.Y * canvasesContainer.Height);
                    pointerContainer.Center = new Point(x, y);
                    if (((RemotePointer)pointerContainer.Tag).IsActive)
                    {
                        (await _eventLogger).UploadLogString(BrainstormingEventLogger.getLogStr_RemotePointerMoved(updatedPointer));
                    }
                    pointerContainer.Tag = updatedPointer;
                    sv_RemotePointerCanvas.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                UtilitiesLib.LogError(ex);
            }
        }

        #endregion Public Methods

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
                _noteUpdateScheduler.Stop();
                _session.Close();
            }
            catch (Exception ex)
            {
                UtilitiesLib.LogError(ex);
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private void addedIdeaUI_colorPaletteLaunchedEventHandler(object sender, float posX, float posY)
        {
            var colorPalette = new PostItColorPalette();
            var control = sender as Control;
            if (control != null)
                colorPalette.setSize(control.Width, control.Height);
            colorPalette.CallingControl = control;
            colorPalette.colorPickedEventHandler += colorPalette_colorPickedEventHandler;
            colorPalette.selectedColorApprovedHandler += colorPalette_selectedColorApprovedHandler;
            mainGrid.Children.Add(colorPalette);
            var paletteMargin = colorPalette.Margin;
            paletteMargin.Left = posX - colorPalette.Width / 2;
            paletteMargin.Top = posY - colorPalette.Height / 2;
            colorPalette.Margin = paletteMargin;
            colorPalette.HorizontalAlignment = HorizontalAlignment.Left;
            colorPalette.VerticalAlignment = VerticalAlignment.Top;
        }

        private async void addedIdeaUI_noteUISizeChangedListener(object sender, IdeationUnit associatedIdea, float scaleX, float scaleY)
        {
            await TakeASnapshot();
            (await _eventLogger).UploadLogString(BrainstormingEventLogger.getLogStr_NoteSizeChanged(associatedIdea, scaleX, scaleY));
        }

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
            }), ideas, asInit);
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
                container.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                container.Width = groupUi.Width;
                container.Height = groupUi.Height;
                container.Orientation = 0;
                container.CanScale = false;
                container.CanRotate = false;
                container.Center = new Point(ideaGroup.CenterX, ideaGroup.CenterY);
                groupUi.InitContainer(container);
                sv_MainCanvas.Items.Add(container);
            }
            catch (Exception ex)
            {
                UtilitiesLib.LogError(ex);
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
                    container.Center = new Point(idea.CenterX, idea.CenterY);
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
                    addedIdeaUi.noteUITranslatedEventHandler += noteUIManipluatedEventHandler;
                    addedIdeaUi.noteUIDeletedEventHandler += noteUIDeletedEventHandler;
                    addedIdeaUi.noteUISizeChangedListener += addedIdeaUI_noteUISizeChangedListener;
                    addedIdeaUi.colorPaletteLaunchedEventHandler += addedIdeaUI_colorPaletteLaunchedEventHandler;
                }
                (await _eventLogger).UploadLogString(BrainstormingEventLogger.getLogStr_NoteAdded(idea));
            }
            catch (Exception ex)
            {
                UtilitiesLib.LogError(ex);
            }
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
                    newStroke.DrawingAttributes.Color = (Color)ColorConverter.ConvertFromString(ideaData.StrokeColorCode);
                }
                else
                {
                    newStroke.DrawingAttributes = new DrawingAttributes();
                    newStroke.DrawingAttributes.Color = Color.FromRgb(0, 0, 0);
                    newStroke.DrawingAttributes.Width = newStroke.DrawingAttributes.Height = 30;
                }
                drawingCanvas.Strokes.Add(newStroke);
            }
            catch (Exception ex)
            {
                UtilitiesLib.LogError(ex);
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
            sv_MainCanvas.Width = ActualWidth;
            sv_MainCanvas.Height = ActualHeight;
            drawingCanvas.Width = ActualWidth;
            drawingCanvas.Height = ActualHeight;
            canvasesContainer.Width = ActualWidth;
            canvasesContainer.Height = ActualHeight;
            mainGrid.Width = ActualWidth;
            mainGrid.Height = ActualHeight;

            LayoutMenu();

            Loaded -= BrainstormCanvas_Loaded;
        }

        private async void brainstormManager_ideaAddedEventHandler(IdeationUnit addedIdea)
        {
            // lock (sync)
            {
                var oneItemList = new List<IdeationUnit>();
                oneItemList.Add(addedIdea);
                AddNewIdeaUIs(oneItemList, true);
                if (_initializationFinished)
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
                (await _eventLogger).UploadLogString(BrainstormingEventLogger.getLogStr_TimelineFrameFinishRetrieving());
                await TakeASnapshot();
            }
        }

        private async void brainstormManager_ideaRemovedHandler(IdeationUnit removedIdea)
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
                (await _eventLogger).UploadLogString(BrainstormingEventLogger.getLogStr_NoteRestored(restoredIdea));
            }
        }

        private async void brainstormManager_ideaUIColorChangeHandler(IdeationUnit updatedIdea, string colorCode)
        {
            await TakeASnapshot();
            _timelineManager.AddCOLORChange(updatedIdea.Id, colorCode);
        }

        private async void brainstormManager_ideaUpdatedHandler(IdeationUnit updatedIdea, IdeationUnit.IdeaUpdateType updateType)
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

        private async void ButtonNewSession_Click(object sender, RoutedEventArgs e)
        {
            _session = new Session(TextBoxSessionName.Text);
            await _session.CreateSessionAsync();

            await InitNetworkCommManager();

            CloseSessionManager();
        }

        private async void ButtonOpenSession_Click(object sender, RoutedEventArgs e)
        {
            // For test only
            _session = new Session(ListBoxSessions.SelectedItem as string);
            await _session.GetSessionAsync();            

            await InitNetworkCommManager();
            CloseSessionManager();
        }

        private void ButtonSessionManager_Click(object sender, RoutedEventArgs e)
        {
            OpenSessionManager();
        }

        private void ClearNotes()
        {
            Dispatcher.Invoke(() =>
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
                    UtilitiesLib.LogError(ex);
                }
            });
        }

        private void CloseSessionManager()
        {
            //StackPanelSessionManager.IsEnabled = false;
            var animation = new DoubleAnimation(0.0d, _fadeDuration);
            StackPanelSessionManager.BeginAnimation(StackPanel.OpacityProperty, animation);
            animation.Completed += (o, e) => StackPanelSessionManager.Visibility = Visibility.Hidden;
        }

        private void colorPalette_colorPickedEventHandler(Control callingControl, string colorCode)
        {
            var imageBasedPostItUi = callingControl as ImageBasedPostItUI;
            imageBasedPostItUi?.setBackgroundPostItColor(colorCode);
        }

        private async void colorPalette_selectedColorApprovedHandler(object sender, Control callingControl, string approvedColorCode)
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
                var pathPoints = new List<Point>();
                foreach (var stylusP in strokePoints)
                {
                    var p = new Point();
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
                strokeData.StrokeColorCode = new ColorConverter().ConvertToString(latestStroke.DrawingAttributes.Color);
                strokeData.StrokePoints = pathPoints;
                strokeIdea.Content = strokeData;
                _brainstormManager.AddIdeaInBackground(strokeIdea);
                //get the current screenshot
                await TakeASnapshot();
                _timelineManager.AddADDChange(strokeIdea);
            }
            catch (Exception ex)
            {
                UtilitiesLib.LogError(ex);
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

        private ScatterViewItem FindViewItemWithPointerId(int pointerId)
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

        private async void GeneralNoteDownloaderOnInitializationFinished(object sender, EventArgs eventArgs)
        {
            _initializationFinished = true;
            await TakeASnapshot();
        }

        private void InitBrainstormingProcessors()
        {
            _brainstormManager = new PostItGeneralManager();
            _brainstormManager.ideaAddedEventHandler += brainstormManager_ideaAddedEventHandler;
            _brainstormManager.ideaUpdatedHandler += brainstormManager_ideaUpdatedHandler;
            _brainstormManager.ideaRemovedHandler += brainstormManager_ideaRemovedHandler;
            _brainstormManager.ideaRestoredHandler += brainstormManager_ideaRestoredHandler;
            _brainstormManager.ideaUIColorChangeHandler += brainstormManager_ideaUIColorChangeHandler;
            _brainstormManager.ideaCollectionRollBackFinishedEventHandler += brainstormManager_ideaCollectionRollBackFinishedEventHandler;

            _brainstormManager.TrashManager.discardedIdeaReceivedEventHandler += recycleBin.AddDiscardedIdea;

            recycleBin.noteRestoredEventHandler += _brainstormManager.TrashManager.RestoreIdea;
        }

        private async Task InitNetworkCommManager()
        {
            //processors related to cloud service
            //_session.AllNotesDownloaded += GeneralNoteDownloaderOnInitializationFinished;
            _noteUpdateScheduler = new NoteUpdateScheduler();

            _cloudDataEventProcessor = new CloudDataEventProcessor();
            
            _session.NewNoteDownloaded += _cloudDataEventProcessor.HandleDownloadedStreamsFromCloud;
            _cloudDataEventProcessor.NewNoteExtractedEventHandler += _brainstormManager.HandleComingIdea;
            await _session.UpdateNotes();
            _noteUpdateScheduler.updateEventHandler += noteUpdateScheduler_updateEventHandler;

            _p2PClient = new AsyncTCPClient();
            _remotePointerManager = new RemotePointerManager();
            _p2PClient.SetP2PDataListener(_remotePointerManager);
            _remotePointerManager.setPointerEventListener(this);
            try
            {
                _p2PClient.StartClient();
            }
            catch (SocketException)
            {
                MessageBox.Show(Properties.Resources.Cannot_connect_to_the_remote_pointer_msg,
                    "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void InitTimeline()
        {
            timelineView.frameSelectedEventHandler += timelineView_frameSelectedEventHandler;
            _timelineManager = new TimelineChangeManager();
            _timelineManager.newTimelineFrameAddedEventHandler += newTimelineFrameAddedEventHandler;
            _timelineManager.startEnumeratingEventHandler += _brainstormManager.reset;
            _timelineManager.finishEnumeratingEventHandler += _brainstormManager.notifyIdeaCollectionRollBack;

            var eventIntepreter = new TimlineEventIntepreter();
            eventIntepreter.ADDeventExtractedHandler += _brainstormManager.AddIdeaInBackground;
            eventIntepreter.REMOVEeventExtractedHandler += _brainstormManager.RemoveIdeaInBackground;
            eventIntepreter.RESTOREeventExtractedHandler += _brainstormManager.RestoreIdeaInBackground;
            eventIntepreter.UPDATEPosEventExtractedHandler += _brainstormManager.UpdateIdeaPositionInBackground;
            eventIntepreter.UPDATEContentEventExtractedHandler += _brainstormManager.UpdateIdeaContentInBackground;
            eventIntepreter.COLORChangeEventExtractedHandler += _brainstormManager.ChangeIdeaUIColorInBackground;
            _timelineManager.EventIntepreter = eventIntepreter;
        }

        private void LayoutMenu()
        {
            //MainMenu.Radius = this.Height * 0.15;
            MainMenu.HorizontalAlignment = HorizontalAlignment.Left;
            MainMenu.VerticalAlignment = VerticalAlignment.Top;
            var mainMenuMargin = MainMenu.Margin;
            mainMenuMargin.Left = ActualWidth / 2 - MainMenu.Radius;
            mainMenuMargin.Top = ActualHeight - 2.25 * MainMenu.Radius;
            MainMenu.Margin = mainMenuMargin;
        }

        private async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            (await _eventLogger).Close();
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            // Add handlers for window availability events
            _eventLogger = BrainstormingEventLogger.GetInstance(_backendStorage);
            AddWindowAvailabilityHandlers();
            InitBrainstormingProcessors();
            InitTimeline();

            OpenSessionManager();
            // 


            DrawingCanvasModeSwitcher.normalDrawingAttribute = drawingCanvas.DefaultDrawingAttributes.Clone();

            var workingArea = Screen.AllScreens[Settings.Default.ActiveWorkingScreen].WorkingArea;
            Left = workingArea.Left;
            Top = workingArea.Top;
            Width = workingArea.Width;
            Height = workingArea.Height;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
            (await _eventLogger).UploadLogString(BrainstormingEventLogger.getLogStr_Start());
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LayoutMenu();
        }

        private void MenuItemDrawingSwitch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DrawingCanvasModeSwitcher.Flip();
                if (DrawingCanvasModeSwitcher.IsInErasingMode())
                {
                    drawingCanvas.UsesTouchShape = false;
                    drawingCanvas.DefaultDrawingAttributes.Color = Color.FromRgb(0, 0, 0);
                    drawingCanvas.DefaultDrawingAttributes.Width = drawingCanvas.DefaultDrawingAttributes.Height = 30;
                    MenuItemDrawingSwitch.Header = MainWindow.Resources["PencilIcon"];
                }
                else
                {
                    drawingCanvas.UsesTouchShape = true;
                    drawingCanvas.DefaultDrawingAttributes = DrawingCanvasModeSwitcher.normalDrawingAttribute.Clone();
                    MenuItemDrawingSwitch.Header = MainWindow.Resources["EraserIcon"];
                }
            }
            catch (Exception ex)
            {
                UtilitiesLib.LogError(ex);
            }
        }

        private void MenuItemRecycleBin_Click(object sender, RoutedEventArgs e)
        {
            if (timelineView.Visibility == Visibility.Visible)
            {
                timelineView.FadeOut();
            }
            if (recycleBin.Visibility == Visibility.Visible)
            {
                recycleBin.FadeOut();
            }
            else
            {
                recycleBin.FadeIn();
            }
        }

        private void MenuItemTimeline_Click(object sender, RoutedEventArgs e)
        {
            if (recycleBin.Visibility == Visibility.Visible)
            {
                recycleBin.FadeOut();
            }
            if (timelineView.Visibility == Visibility.Hidden)
            {
                timelineView.FadeIn();
            }
            else
            {
                timelineView.FadeOut();
            }
        }

        private void newTimelineFrameAddedEventHandler(TimelineFrame addedFrame)
        {
            Dispatcher.Invoke(new Action<TimelineFrame>(frameToAdd =>
            {
                timelineView.AddFrame(frameToAdd);
            }), addedFrame);
        }

        private void noteUIDeletedEventHandler(object sender, IdeationUnit associatedIdea)
        {
            _brainstormManager.RemoveIdea(associatedIdea);
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
            await _session.UpdateNotes();
            //await _anotoNotesDownloader.UpdateNotes();
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


        private async void OpenSessionManager()
        {
            var sessions = await Session.GetSessionNames();
            ListBoxSessions.Items.Clear();
            foreach (var s in sessions)
                ListBoxSessions.Items.Add(s);

            var animation = new DoubleAnimation(1.0d, _fadeDuration);
            StackPanelSessionManager.Visibility = Visibility.Visible;
            StackPanelSessionManager.BeginAnimation(StackPanel.OpacityProperty, animation);
            //animation.Completed += (o,e) => StackPanelSessionManager.IsEnabled = true;
        }

        private void RefreshBrainstormingWhiteboard()
        {
            Dispatcher.Invoke(() =>
            {
                sv_MainCanvas.UpdateLayout();
            });
        }

        private async void RemoveNoteUi(IdeationUnit associatedIdea)
        {
            try
            {
                var ideaContainer = FindNoteContainerOfIdea(associatedIdea);
                sv_MainCanvas.Items.Remove(ideaContainer);
                (await _eventLogger).UploadLogString(BrainstormingEventLogger.getLogStr_NoteDeleted(associatedIdea));
            }
            catch (Exception ex)
            {
                UtilitiesLib.LogError(ex);
            }
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

        private void SurfaceButton_Click(object sender, RoutedEventArgs e)
        {
            CloseSessionManager();
        }

        private void sv_MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseDown(e);
            sv_MainCanvas.IsHitTestVisible = false;
        }

        private void sv_MainCanvas_TouchDown(object sender, TouchEventArgs e)
        {
            OnTouchDown(e);
            sv_MainCanvas.IsHitTestVisible = false;
        }

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
                    var resizedNoteContainerBmpFrame = UtilitiesLib.CreateResizedBitmapFrame(noteContainerImgSrc, (int)(canvasesContainer.ActualWidth * 3 / 4), (int)(canvasesContainer.Height * 3 / 4), 0);
                    var imageEncoder = new PngBitmapEncoder();
                    imageEncoder.Frames.Add(BitmapFrame.Create(resizedNoteContainerBmpFrame));
                    //imageEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                    var screenshotBytes = new byte[1];
                    using (var stream = new MemoryStream())
                    {
                        imageEncoder.Save(stream);
                        stream.Seek(0, 0);
                        screenshotBytes = stream.ToArray();
                        GlobalObjects.currentScreenshotBytes = screenshotBytes;

                        var boardScreenUpdater = BoardScreenUpdater.GetInstance(_backendStorage, _snapshotFolder);

                        await boardScreenUpdater.UpdateMetaplanBoardScreen(new MemoryStream(screenshotBytes));

                        //bgUploader.RunWorkerAsync(new MemoryStream(screenshotBytes));
                    }
                }
                catch (Exception ex)
                {
                    UtilitiesLib.LogError(ex);
                }
            });
        }

        private async void timelineView_frameSelectedEventHandler(int selectedFrameId)
        {
            (await _eventLogger).UploadLogString(BrainstormingEventLogger.getLogStr_TimelineFrameStartRetrieving(selectedFrameId));
            _timelineManager.SelectFrame(selectedFrameId);
        }

        private void UpdateNoteUiContent(IdeationUnit updatedIdea)
        {
            Dispatcher.Invoke(new Action<IdeationUnit>(ideaToUpdate =>
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
                    UtilitiesLib.LogError(ex);
                }
            }), updatedIdea.Clone());
        }

        private async Task UpdateNoteUiPosition(IdeationUnit updatedIdea)
        {
            try
            {
                var noteContainer = FindNoteContainerOfIdea(updatedIdea);
                if (noteContainer != null)
                {
                    noteContainer.Center = new Point(updatedIdea.CenterX, updatedIdea.CenterY);
                    (await _eventLogger).UploadLogString(BrainstormingEventLogger.getLogStr_NoteMoved(updatedIdea));
                }
            }
            catch (Exception ex)
            {
                UtilitiesLib.LogError(ex);
            }
        }

        #endregion Private Methods

        #region Private Fields

        private static ICloudFS<File> _backendStorage = Session.Storage;
        private static object _sync = new object();
        private readonly Duration _fadeDuration = new Duration(TimeSpan.FromSeconds(2));
        private PostItGeneralManager _brainstormManager;
        private CloudDataEventProcessor _cloudDataEventProcessor;

        private Task<BrainstormingEventLogger> _eventLogger;
        private bool _initializationFinished;
        private object _lock = new object();

        //Network data processors
        private NoteUpdateScheduler _noteUpdateScheduler;

        private AsyncTCPClient _p2PClient;
        private RemotePointerManager _remotePointerManager;
        private Session _session;
        private File _snapshotFolder;

        //PostItNetworkDataManager networkDataManager = null;
        //Timeline processors
        private TimelineChangeManager _timelineManager;

        #endregion Private Fields

        /* Runs on UI thread */
    }
}