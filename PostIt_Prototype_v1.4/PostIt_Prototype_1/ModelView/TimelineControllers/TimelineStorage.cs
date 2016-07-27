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
        XmlDocument _xmlDoc = null;
        string _currentFileName = string.Empty;
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
                _xmlDoc = new XmlDocument();
                var xmlDeclaration = _xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                //create root
                var rootNode = _xmlDoc.CreateElement("TIMELINE");
                _xmlDoc.InsertBefore(xmlDeclaration, _xmlDoc.DocumentElement);
                _xmlDoc.AppendChild(rootNode);

                var fullFilePath = timelineFolder + "/" + fileName;
                _currentFileName = fullFilePath;
                if (!File.Exists(fullFilePath))
                {
                    var fs = File.Create(fullFilePath);
                    fs.Close();
                }
                _xmlDoc.Save(fullFilePath);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        public void SaveFrame(TimelineFrame frame)
        {
            try
            {
                var xmlNode = frame.ToXml(_xmlDoc.DocumentElement);
                _xmlDoc.DocumentElement.AppendChild(xmlNode);
                _xmlDoc.Save(_currentFileName);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        public TimelineFrame RetrieveFrameFromStorage(int frameId)
        {
            var queryPath = string.Format("//FRAME[@ID='{0}']",frameId);
            var node = (XmlElement)_xmlDoc.DocumentElement.SelectSingleNode(queryPath);
            if (node != null)
            {
                return TimelineFrame.ExtractTimelineFrameFromXmlNode(node);
            }
            return null;
        }
    }
}
