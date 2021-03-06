﻿using System;
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
        public XmlElement toXML(XmlElement parentNode)
        {
            XmlElement frame = parentNode.OwnerDocument.CreateElement("FRAME");
            XmlAttribute frameIDAttr = parentNode.OwnerDocument.CreateAttribute("ID");
            frameIDAttr.Value = _id.ToString();
            frame.Attributes.Append(frameIDAttr);
            XmlElement containedChange = _change.toXml(frame);
            frame.AppendChild(containedChange);
            return frame;
        }
        public static TimelineFrame extractTimelineFrameFromXmlNode(XmlElement node)
        {
            TimelineFrame frame = new TimelineFrame();
            frame.Id = Int32.Parse(node.Attributes["ID"].Value,CultureInfo.InvariantCulture);
            TimelineChange change = TimelineChange.extractTimelineChangeFromXmlNode((XmlElement)node.FirstChild);
            frame.Change = change;
            return frame;
        }
        
    }
}
