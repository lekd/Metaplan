using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;

namespace PostIt_Prototype_1.TimelineControllers
{
    public class TimelineFrame
    {
        int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        TimelineChange _change;

        public TimelineChange Change
        {
            get { return _change; }
            set { _change = value; }
        }
        public XmlElement ToXml(XmlElement parentNode)
        {
            var frame = parentNode.OwnerDocument.CreateElement("FRAME");
            var frameIdAttr = parentNode.OwnerDocument.CreateAttribute("ID");
            frameIdAttr.Value = _id.ToString();
            frame.Attributes.Append(frameIdAttr);
            var containedChange = _change.ToXml(frame);
            frame.AppendChild(containedChange);
            return frame;
        }
        public static TimelineFrame ExtractTimelineFrameFromXmlNode(XmlElement node)
        {
            var frame = new TimelineFrame();
            frame.Id = Int32.Parse(node.Attributes["ID"].Value,CultureInfo.InvariantCulture);
            var change = TimelineChange.ExtractTimelineChangeFromXmlNode((XmlElement)node.FirstChild);
            frame.Change = change;
            return frame;
        }
        
    }
}
