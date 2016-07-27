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
using PostIt_Prototype_1.ModelView.PostItObjects;

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for RecycleBinUI.xaml
    /// </summary>
    public partial class RecycleBinUi : UserControl
    {
        public event Recycle_Bin.RecycleBinManager.DiscardedIdeaRestored NoteRestoredEventHandler = null;
        public RecycleBinUi()
        {
            InitializeComponent();
        }
        public void AddDiscardedIdea(GenericIdeationObjects.IdeationUnit idea)
        {
            try
            {
                IPostItUi addedIdeaUi = null;
                if (idea is PostItNote)
                {
                    var castNote = (PostItNote)idea;
                    if (castNote.Content is Bitmap)
                    {

                        var noteUi = new ImageBasedPostItUi();
                        noteUi.Tag = idea;
                        noteUi.SetNoteId(castNote.Id);
                        noteUi.UpdateDisplayedContent(((Bitmap)castNote.Content).Clone());
                        noteUi.Width = noteUi.Height = this.Height * 2 / 3;
                        noteUi.DisableZoomButtons();
                        addedIdeaUi = noteUi;
                    }
                }
                if (addedIdeaUi != null)
                {
                    addedIdeaUi.NoteUiDeletedEventHandler += new NoteUiDeletedEvent(ideaUI_noteUIDeletedEventHandler);
                    this.DiscardedItemsContainer.Children.Add((UserControl)addedIdeaUi);
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
                DiscardedItemsContainer.Children.Clear();
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
                var noteToRestore = GetIdeaUiWithId(associatedIdea.Id);
                if (noteToRestore != null)
                {
                    DiscardedItemsContainer.Children.Remove((Control)noteToRestore);
                    if (NoteRestoredEventHandler != null)
                    {
                        NoteRestoredEventHandler(associatedIdea);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        IPostItUi GetIdeaUiWithId(int id)
        {
            for (var i = 0; i < DiscardedItemsContainer.Children.Count; i++)
            {
                var currentUi = (IPostItUi)DiscardedItemsContainer.Children[i];
                if (currentUi.GetNoteId() == id)
                {
                    return currentUi;
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
