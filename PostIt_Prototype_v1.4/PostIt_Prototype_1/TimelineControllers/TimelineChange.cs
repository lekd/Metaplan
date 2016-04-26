using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Drawing;
using System.Globalization;
using System.Windows.Input;
using PostIt_Prototype_1.PostItObjects;

namespace PostIt_Prototype_1.TimelineControllers
{
    public enum TypeOfChange
    {
        ADD, DELETE, RESTORE, UPDATE, DUPLICATE
    }
    public class TimelineChange
    {
        TypeOfChange _changeType;

        public TypeOfChange ChangeType
        {
            get { return _changeType; }
            set { _changeType = value; }
        }
        int _changedIdeaID;

        public int ChangedIdeaID
        {
            get { return _changedIdeaID; }
            set { _changedIdeaID = value; }
        }
        object _metaData;

        public object MetaData
        {
            get { return _metaData; }
            set { _metaData = value; }
        }
        public string getChangeTypeString()
        {
            switch (_changeType)
            {
                case TypeOfChange.ADD:
                    return "ADD";
                case TypeOfChange.UPDATE:
                    return "UPDATE";
                case TypeOfChange.DELETE:
                    return "DELETE";
                case TypeOfChange.RESTORE:
                    return "RESTORE";
                case TypeOfChange.DUPLICATE:
                    return "DUPLICATE";
            }
            return string.Empty;
        }
        public TimelineChange()
        {

        }
        public TimelineChange(TypeOfChange changeType, int targetNoteId, object changedData)
        {
            _changeType = changeType;
            _changedIdeaID = targetNoteId;
            _metaData = changedData;
        }
        public static string getUpdateTypeString(object updateMetaData)
        {
            if(updateMetaData is System.Windows.Point)
            {
                return "POS";
            }
            return "CONTENT";
        }
        public XmlElement toXml(XmlElement parentNode)
        {
            switch (_changeType)
            {
                case TypeOfChange.ADD:
                    return getADDCommandXml(this, parentNode);
                case TypeOfChange.DELETE:
                    return getDELETECommandXml(this, parentNode);
                case TypeOfChange.RESTORE:
                    return getRESTORECommandXml(this, parentNode);
                case TypeOfChange.UPDATE:
                    return getUPDATECommandXml(this, parentNode);
                case TypeOfChange.DUPLICATE:
                    return getDUPLICATECommandXml(this, parentNode);
            }
            return null;
        }
        static XmlElement getADDCommandXml(TimelineChange change, XmlElement parentNode)
        {
            /*
             * Xml node structure
             * <ADD NoteID=" " ContentType="[BITMAP/STROKE/GROUP]" Content=" "/>
             */
            
            try
            {
                XmlElement node = parentNode.OwnerDocument.CreateElement(change.getChangeTypeString());
                XmlAttribute ideaIDAttr = parentNode.OwnerDocument.CreateAttribute("NoteID");
                ideaIDAttr.Value = change.ChangedIdeaID.ToString();
                node.Attributes.Append(ideaIDAttr);
                XmlAttribute ideaContentTypeAttr = parentNode.OwnerDocument.CreateAttribute("ContentType");
                XmlAttribute ideaContentAttr = parentNode.OwnerDocument.CreateAttribute("Content");
                if (change.MetaData is Bitmap)
                {
                    ideaContentTypeAttr.Value = "BITMAP";
                    ideaContentAttr.Value = PostItObjects.PostItNote.getDatatStringOfIdeaContent(change.MetaData);
                }
                else if (change.MetaData is List<int>)
                {
                    ideaContentAttr.Value = "GROUP";
                }
                else if (change.MetaData is StrokeData)
                {

                    ideaContentTypeAttr.Value = "STROKE";
                    ideaContentAttr.Value = (change.MetaData as StrokeData).getStringFromStrokePoints();
                    XmlAttribute isErasingAttr = parentNode.OwnerDocument.CreateAttribute("IsErasing");
                    isErasingAttr.Value = (change.MetaData as StrokeData).getStringOfIsErasingAttribute();
                    node.Attributes.Append(isErasingAttr);
                    XmlAttribute strokeColorAttr = parentNode.OwnerDocument.CreateAttribute("Color");
                    strokeColorAttr.Value = (change.MetaData as StrokeData).StrokeColorCode;
                    node.Attributes.Append(strokeColorAttr);
                }
                node.Attributes.Append(ideaContentTypeAttr);
                node.Attributes.Append(ideaContentAttr);
                return node;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static XmlElement getDELETECommandXml(TimelineChange change, XmlElement parentNode)
        {
            /*
             * Xml node structure
             * <DELETE NoteID=" "/>
             */
            try
            {
                XmlElement node = parentNode.OwnerDocument.CreateElement(change.getChangeTypeString());
                XmlAttribute ideaIDAttr = parentNode.OwnerDocument.CreateAttribute("NoteID");
                ideaIDAttr.Value = change.ChangedIdeaID.ToString();
                node.Attributes.Append(ideaIDAttr);
                return node;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static XmlElement getRESTORECommandXml(TimelineChange change, XmlElement parentNode)
        {
            /*
             * Xml node structure
             * <RESTORE NoteID=" "/>
             */
            try
            {
                XmlElement node = parentNode.OwnerDocument.CreateElement(change.getChangeTypeString());
                XmlAttribute ideaIDAttr = parentNode.OwnerDocument.CreateAttribute("NoteID");
                ideaIDAttr.Value = change.ChangedIdeaID.ToString();
                node.Attributes.Append(ideaIDAttr);
                return node;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static XmlElement getDUPLICATECommandXml(TimelineChange change, XmlElement parentNode)
        {
            /*
             * Xml structure
             * <DUPLICATE RefID=" "/>
             */
            try
            {
                XmlElement node = parentNode.OwnerDocument.CreateElement(change.getChangeTypeString());
                XmlAttribute refIDAttr = parentNode.OwnerDocument.CreateAttribute("RefID");
                refIDAttr.Value = change.ChangedIdeaID.ToString();
                node.Attributes.Append(refIDAttr);
                return node;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static XmlElement getUPDATECommandXml(TimelineChange change, XmlElement parentNode)
        {
            /*
             * Xml structure
             * if update position
             * <UPDATE NoteID=" " UpdateType="POS" X=" " Y=" " />
             * if update content
             * <UPDATE NoteID=" " UpdateType="CONTENT" Content=" " />
             */
            try
            {
                XmlElement node = parentNode.OwnerDocument.CreateElement(change.getChangeTypeString());
                XmlAttribute ideaIDAttr = parentNode.OwnerDocument.CreateAttribute("NoteID");
                ideaIDAttr.Value = change.ChangedIdeaID.ToString();
                node.Attributes.Append(ideaIDAttr);
                XmlAttribute updateTypeAttr = parentNode.OwnerDocument.CreateAttribute("UpdateType");
                updateTypeAttr.Value = getUpdateTypeString(change.MetaData);
                node.Attributes.Append(updateTypeAttr);
                if (updateTypeAttr.Value.CompareTo("POS") == 0)
                {
                    System.Windows.Point newPos = (System.Windows.Point)change.MetaData;
                    XmlAttribute posXAttr = parentNode.OwnerDocument.CreateAttribute("X");
                    posXAttr.Value = newPos.X.ToString();
                    node.Attributes.Append(posXAttr);

                    XmlAttribute posYAttr = parentNode.OwnerDocument.CreateAttribute("Y");
                    posYAttr.Value = newPos.Y.ToString();
                    node.Attributes.Append(posYAttr);
                }
                else if (updateTypeAttr.Value.CompareTo("CONTENT") == 0)
                {
                    XmlAttribute contentAttr = parentNode.OwnerDocument.CreateAttribute("Content");
                    contentAttr.Value = PostItObjects.PostItNote.getDatatStringOfIdeaContent(change.MetaData);
                    node.Attributes.Append(contentAttr);
                }
                return node;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        public static TimelineChange extractTimelineChangeFromXmlNode(XmlElement node)
        {
            
            if (node.Name.CompareTo("ADD") == 0)
            {
                return extractADDCommandFromXmlNode(node);
            }
            if (node.Name.CompareTo("DELETE") == 0)
            {
                return extractDELETECommandFromXmlNode(node);
            }
            if (node.Name.CompareTo("RESTORE") == 0)
            {
                return extractDELETECommandFromXmlNode(node);
            }
            if (node.Name.CompareTo("UPDATE") == 0)
            {
                return extractUPDATECommandFromXmlNode(node);
            }
            if (node.Name.CompareTo("DUPLICATE") == 0)
            {
                return extractDUPLICATECommandFromXmlNode(node);
            }
            return null;
        }
        static TimelineChange extractADDCommandFromXmlNode(XmlElement node)
        {
            try
            {
                TimelineChange ADDchange = new TimelineChange();
                ADDchange.ChangeType = TypeOfChange.ADD;
                ADDchange.ChangedIdeaID = Int32.Parse(node.Attributes["NoteID"].Value, CultureInfo.InvariantCulture);
                string contentType = node.Attributes["ContentType"].Value;
                string contentStr = node.Attributes["Content"].Value;
                if (contentStr.CompareTo("BITMAP") == 0)
                {
                    byte[] contentBytes = Convert.FromBase64String(contentStr);
                    ADDchange.MetaData = Utilities.UtilitiesLib.BytesToBitmap(contentBytes);
                }
                else if (contentStr.CompareTo("STROKE") == 0)
                {
                    //ADDchange.MetaData = PostItObjects.StrokeBasedIdea.ParseContentFromString(contentStr);
                    StrokeData strokeData = new StrokeData();
                    strokeData.ParseStrokePointsFromString(contentStr);
                    if (node.HasAttribute("IsErasing"))
                    {
                        strokeData.ParseIsErasingFromString(node.Attributes["IsErasing"].Value);
                    }
                    if (node.HasAttribute("Color"))
                    {
                        strokeData.StrokeColorCode = node.Attributes["Color"].Value;
                    }
                    ADDchange.MetaData = strokeData;
                }
                else if (contentStr.CompareTo("GROUP") == 0)
                {

                }
                return ADDchange;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static TimelineChange extractDELETECommandFromXmlNode(XmlElement node)
        {
            try
            {
                TimelineChange DELchange = new TimelineChange();
                DELchange.ChangeType = TypeOfChange.DELETE;
                DELchange.ChangedIdeaID = Int32.Parse(node.Attributes["NoteID"].Value, CultureInfo.InvariantCulture);
                return DELchange;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static TimelineChange extractRESTORECommandFromXmlNode(XmlElement node)
        {
            try
            {
                TimelineChange DELchange = new TimelineChange();
                DELchange.ChangeType = TypeOfChange.RESTORE;
                DELchange.ChangedIdeaID = Int32.Parse(node.Attributes["NoteID"].Value, CultureInfo.InvariantCulture);
                return DELchange;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static TimelineChange extractDUPLICATECommandFromXmlNode(XmlElement node)
        {
            try
            {
                TimelineChange DUPLchange = new TimelineChange();
                DUPLchange.ChangeType = TypeOfChange.DUPLICATE;
                DUPLchange.ChangedIdeaID = Int32.Parse(node.Attributes["RefID"].Value, CultureInfo.InvariantCulture);
                return DUPLchange;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static TimelineChange extractUPDATECommandFromXmlNode(XmlElement node)
        {
            try
            {
                TimelineChange UPDATEchange = new TimelineChange();
                UPDATEchange.ChangeType = TypeOfChange.UPDATE;
                UPDATEchange.ChangedIdeaID = Int32.Parse(node.Attributes["NoteID"].Value, CultureInfo.InvariantCulture);
                string updateType = node.Attributes["UpdateType"].Value;
                if (updateType.CompareTo("POS") == 0)
                {
                    float X = float.Parse(node.Attributes["X"].Value, CultureInfo.InvariantCulture);
                    float Y = float.Parse(node.Attributes["Y"].Value, CultureInfo.InvariantCulture);
                    System.Windows.Point p = new System.Windows.Point(X, Y);
                    UPDATEchange.MetaData = p;
                }
                else if (updateType.CompareTo("CONTENT") == 0)
                {
                    string contentStr = node.Attributes["Content"].Value;
                    byte[] contentBytes = Convert.FromBase64String(contentStr);
                    UPDATEchange.MetaData = Utilities.UtilitiesLib.BytesToBitmap(contentBytes);
                }
                return UPDATEchange;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
            
        }
    }
}
