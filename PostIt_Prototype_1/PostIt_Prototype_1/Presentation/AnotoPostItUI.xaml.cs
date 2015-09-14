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
using PostIt_Prototype_1.PostItDataHandlers;
using System.Windows.Ink;

namespace PostIt_Prototype_1.Presentation
{
    /// <summary>
    /// Interaction logic for DrawablePostItUI.xaml
    /// </summary>
    public partial class AnotoPostItUI : UserControl, IPostItUI
    {
        int _noteID;
        int _initWidth = 150;

        public int InitWidth
        {
            get { return _initWidth; }
            set { _initWidth = value; }
        }
        int _initHeight = 150;

        public int InitHeight
        {
            get { return _initHeight; }
            set { _initHeight = value; }
        }
        public int getNoteID()
        {
            return _noteID;
        }
        public void setNoteID(int id)
        {
            _noteID = id;
        }
        //for drawing
        
        public AnotoPostItUI()
        {
            InitializeComponent();
        }
        public void InitDrawingVariables()
        {
            
            
        }
        public void updateDisplayedContent(object content)
        {
            redrawContent(content);
        }
        void redrawContent(Object content)
        {
            List<AnotoInkTrace> traces = (List<AnotoInkTrace>)content;
            StrokeCollection strokes = new StrokeCollection();
            foreach (AnotoInkTrace trace in traces)
            {
                strokes.Add(createStrokeFromTrace(trace));
            }
            contentDisplayer.Strokes = strokes;
        }
        Stroke createStrokeFromTrace(AnotoInkTrace trace)
        {
            StylusPointCollection pointCollection = new StylusPointCollection();
            foreach (AnotoInkDot inkDot in trace.InkDots)
            {
                StylusPoint p = new StylusPoint(inkDot.X, inkDot.Y);
                pointCollection.Add(p);
            }
            DrawingAttributes attr = new DrawingAttributes();
            attr.Color = Color.FromRgb(0, 0, 0);
            attr.StylusTip = StylusTip.Ellipse;
            attr.Width = attr.Height = 2;
            return new Stroke(pointCollection,attr);
        }
        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
