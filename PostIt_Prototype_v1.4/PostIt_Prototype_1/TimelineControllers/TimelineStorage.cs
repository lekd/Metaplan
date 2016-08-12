using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace WhiteboardApp.TimelineControllers
{
    public class TimelineStorage
    {
        XmlDocument xmlDoc = null;
        string currentFileName = string.Empty;
        public void Initiate()
        {
            try
            {
                var timelineFolder = Environment.CurrentDirectory + "/Timelines";
                if (!Directory.Exists(timelineFolder))
                {
                    Directory.CreateDirectory(timelineFolder);
                }
                var fileName = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xml";
                xmlDoc = new XmlDocument();
                var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                //create root
                var rootNode = xmlDoc.CreateElement("TIMELINE");
                xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);
                xmlDoc.AppendChild(rootNode);

                var fullFilePath = timelineFolder + "/" + fileName;
                currentFileName = fullFilePath;
                if (!File.Exists(fullFilePath))
                {
                    var fs = File.Create(fullFilePath);
                    fs.Close();
                }
                xmlDoc.Save(fullFilePath);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        public void saveFrame(TimelineFrame frame)
        {
            try
            {
                var xmlNode = frame.toXML(xmlDoc.DocumentElement);
                xmlDoc.DocumentElement.AppendChild(xmlNode);
                xmlDoc.Save(currentFileName);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        public TimelineFrame retrieveFrameFromStorage(int frameID)
        {
            string queryPath = $"//FRAME[@ID='{frameID}']";
            var node = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode(queryPath);
            if (node != null)
            {
                return TimelineFrame.extractTimelineFrameFromXmlNode(node);
            }
            return null;
        }
    }
}
