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
    public partial class TextPostItUi : UserControl
    {
        public delegate void NoteRemovalEventHandler(object sender,TextPostIt toberemovedNote);
        public delegate void NoteUpdateEventHandler(TextPostIt tobeupdatedNote);
        public event NoteRemovalEventHandler RemovalEventHandler;
        public event NoteUpdateEventHandler UpdateEventHandler;

        private TextPostIt _postIt;

        public TextPostIt PostIt
        {
            get { return _postIt; }
            set { _postIt = value; }
        }
        
        public TextPostItUi()
        {
            
            InitializeComponent();
            RemovalEventHandler = null;
            UpdateEventHandler = null;
            _postIt = null;
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            if (RemovalEventHandler != null)
            {
                RemovalEventHandler(this,_postIt);
            }
        }

        private void btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            var btnStatus = (String)BtnEdit.Content;
            if (btnStatus.ToUpper().Equals("EDIT"))
            {
                TxtIdeaContent.IsReadOnly = false;
                TxtIdeaContent.IsHitTestVisible = true;
                TxtIdeaContent.Focus();
                TxtIdeaContent.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                BtnEdit.Background = new SolidColorBrush(Color.FromArgb(255, 73, 158, 30));
                BtnEdit.Content = "Save";
            }
            else
            {
                TxtIdeaContent.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                BtnEdit.Background = new SolidColorBrush(Color.FromArgb(255, 124, 124, 124));
                _postIt.Content = TxtIdeaContent.Text;
                TxtIdeaContent.IsReadOnly = true;
                TxtIdeaContent.IsHitTestVisible = false;
                if (UpdateEventHandler != null)
                {
                    UpdateEventHandler(_postIt);
                }
                BtnEdit.Content = "Edit";
            }
        }
    }
}
