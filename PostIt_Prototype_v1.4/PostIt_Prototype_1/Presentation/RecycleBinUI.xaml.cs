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
using System.Drawing;
using System.Windows.Media.Animation;
using WhiteboardApp.PostItObjects;

namespace WhiteboardApp.Presentation
{
    /// <summary>
    /// Interaction logic for RecycleBinUI.xaml
    /// </summary>
    public partial class RecycleBinUI : UserControl
    {
        public event Recycle_Bin.RecycleBinManager.DiscardedIdeaRestored noteRestoredEventHandler = null;
        public RecycleBinUI()
        {
            InitializeComponent();
        }
        public void AddDiscardedIdea(GenericIdeationObjects.IdeationUnit idea)
        {
            try
            {
                IPostItUI addedIdeaUI = null;
                if (idea is PostItNote)
                {
                    var castNote = (PostItNote)idea;
                    if (castNote.Content is Bitmap)
                    {

                        var noteUI = new ImageBasedPostItUI();
                        noteUI.Tag = idea;
                        noteUI.setNoteID(castNote.Id);
                        noteUI.updateDisplayedContent(((Bitmap)castNote.Content).Clone());
                        noteUI.Width = noteUI.Height = this.Height * 2 / 3;
                        noteUI.DisableZoomButtons();
                        addedIdeaUI = noteUI;
                    }
                }
                if (addedIdeaUI != null)
                {
                    addedIdeaUI.noteUIDeletedEventHandler += new NoteUIDeletedEvent(ideaUI_noteUIDeletedEventHandler);
                    this.discardedItemsContainer.Children.Add((UserControl)addedIdeaUI);
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        public void RefreshNewDiscardedIdeasList(List<GenericIdeationObjects.IdeationUnit> allIdeas)
        {
            try
            {
                discardedItemsContainer.Children.Clear();
                for (var i = 0; i < allIdeas.Count; i++)
                {
                    if (!allIdeas[i].IsAvailable)
                    {
                        AddDiscardedIdea(allIdeas[i]);
                    }
                }
                this.UpdateLayout();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        void ideaUI_noteUIDeletedEventHandler(object sender, GenericIdeationObjects.IdeationUnit associatedIdea)
        {
            try
            {
                var noteToRestore = getIdeaUIWithId(associatedIdea.Id);
                if (noteToRestore != null)
                {
                    discardedItemsContainer.Children.Remove((Control)noteToRestore);
                    if (noteRestoredEventHandler != null)
                    {
                        noteRestoredEventHandler(associatedIdea);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        IPostItUI getIdeaUIWithId(int id)
        {
            for (var i = 0; i < discardedItemsContainer.Children.Count; i++)
            {
                var currentUI = (IPostItUI)discardedItemsContainer.Children[i];
                if (currentUI.getNoteID() == id)
                {
                    return currentUI;
                }
            }
            return null;
        }
        public void FadeIn()
        {
            var anim = new DoubleAnimation(1, TimeSpan.FromSeconds(1));
            this.BeginAnimation(UserControl.OpacityProperty, anim);
            this.Visibility = System.Windows.Visibility.Visible;
        }
        public void FadeOut()
        {
            var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
            this.BeginAnimation(UserControl.OpacityProperty, anim);
            this.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
