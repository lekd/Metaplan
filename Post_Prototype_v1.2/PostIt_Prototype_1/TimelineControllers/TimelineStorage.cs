using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace PostIt_Prototype_1.TimelineControllers
{
    public class TimelineStorage
    {
        XmlDocument xmlDoc = null;
        string currentFileName = string.Empty;
        public void Initiate()
        {
            string timelineFolder = Environment.CurrentDirectory + "/Timelines";
            if (!Directory.Exists(timelineFolder))
            {
                Directory.CreateDirectory(timelineFolder);
            }                               
            string fileName = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xml";
            xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            //create root
            XmlElement rootNode = xmlDoc.CreateElement("TIMELINE");
            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);
            xmlDoc.AppendChild(rootNode);

            string fullFilePath = timelineFolder + "/" + fileName;
            currentFileName = fullFilePath;
            if (!File.Exists(fullFilePath))
            {
                FileStream fs = File.Create(fullFilePath);
                fs.Close();
            }
            xmlDoc.Save(fullFilePath);
        }
        public void saveFrame(TimelineFrame frame)
        {
            XmlElement xmlNode = frame.toXML(xmlDoc.DocumentElement);
            xmlDoc.DocumentElement.AppendChild(xmlNode);
            xmlDoc.Save(currentFileName);
        }
        public TimelineFrame retrieveFrameFromStorage(int frameID)
        {
            string queryPath = string.Format("//FRAME[@ID='{0}']",frameID);
            XmlElement node = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode(queryPath);
            if (node != null)
            {
                return TimelineFrame.extractTimelineFrameFromXmlNode(node);
            }
            return null;
        }
    }
}
