using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;

namespace PostIt_Prototype_1.Presentation
{
    public class DrawingCanvasModeSwitcher
    {
        static bool isErasing = false;
        public static DrawingAttributes normalDrawingAttribute;
        static public void switchEraser(bool isEnabled)
        {
            isErasing = isEnabled;
        }
        static public void Flip()
        {
            isErasing = !isErasing;
        }
        public static bool IsInErasingMode()
        {
            return isErasing;
        }
    }
}
