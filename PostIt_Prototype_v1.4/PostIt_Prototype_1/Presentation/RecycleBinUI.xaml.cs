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
using PostIt_Prototype_1.PostItObjects;
using System.Drawing;
using System.Windows.Media.Animation;

namespace PostIt_Prototype_1.Presentation
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
                    PostItNote castNote = (PostItNote)idea;
                    if (castNote.Content is Bitmap)
                    {

                        ImageBasedPostItUI noteUI = new ImageBasedPostItUI();
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
                for (int i = 0; i < allIdeas.Count; i++)
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
                IPostItUI noteToRestore = getIdeaUIWithId(associatedIdea.Id);
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
            for (int i = 0; i < discardedItemsContainer.Children.Count; i++)
            {
                IPostItUI currentUI = (IPostItUI)discardedItemsContainer.Children[i];
                if (currentUI.getNoteID() == id)
                {
                    return currentUI;
                }
            }
            return null;
        }
        public void FadeIn()
        {
            DoubleAnimation anim = new DoubleAnimation(1, TimeSpan.FromSeconds(1));
            this.BeginAnimation(UserControl.OpacityProperty, anim);
            this.Visibility = System.Windows.Visibility.Visible;
        }
        public void FadeOut()
        {
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
            this.BeginAnimation(UserControl.OpacityProperty, anim);
            this.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
