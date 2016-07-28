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
using Microsoft.Surface.Presentation.Controls;

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for PostItUI.xaml
    /// </summary>
    public partial class TextPostItUI : UserControl
    {
        public delegate void NoteRemovalEventHandler(object sender,TextPostIt toberemoved_note);
        public delegate void NoteUpdateEventHandler(TextPostIt tobeupdated_note);
        public event NoteRemovalEventHandler _removalEventHandler;
        public event NoteUpdateEventHandler _updateEventHandler;

        private TextPostIt _postIt;

        public TextPostIt PostIt
        {
            get { return _postIt; }
            set { _postIt = value; }
        }
        
        public TextPostItUI()
        {
            
            InitializeComponent();
            _removalEventHandler = null;
            _updateEventHandler = null;
            _postIt = null;
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            if (_removalEventHandler != null)
            {
                _removalEventHandler(this,_postIt);
            }
        }

        private void btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            var btnStatus = (String)btn_Edit.Content;
            if (btnStatus.ToUpper().Equals("EDIT"))
            {
                txt_IdeaContent.IsReadOnly = false;
                txt_IdeaContent.IsHitTestVisible = true;
                txt_IdeaContent.Focus();
                txt_IdeaContent.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                btn_Edit.Background = new SolidColorBrush(Color.FromArgb(255, 73, 158, 30));
                btn_Edit.Content = "Save";
            }
            else
            {
                txt_IdeaContent.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                btn_Edit.Background = new SolidColorBrush(Color.FromArgb(255, 124, 124, 124));
                _postIt.Content = txt_IdeaContent.Text;
                txt_IdeaContent.IsReadOnly = true;
                txt_IdeaContent.IsHitTestVisible = false;
                if (_updateEventHandler != null)
                {
                    _updateEventHandler(_postIt);
                }
                btn_Edit.Content = "Edit";
            }
        }
    }
}
