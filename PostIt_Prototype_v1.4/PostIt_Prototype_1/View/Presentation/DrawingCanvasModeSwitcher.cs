using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;

namespace PostIt_Prototype_1.Presentation
{
    public class DrawingCanvasModeSwitcher
    {
        static bool _isErasing = false;
        public static DrawingAttributes NormalDrawingAttribute;
        static public void SwitchEraser(bool isEnabled)
        {
            _isErasing = isEnabled;
        }
        static public void Flip()
        {
            _isErasing = !_isErasing;
        }
        public static bool IsInErasingMode()
        {
            return _isErasing;
        }
    }
}
