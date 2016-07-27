using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Drawing;
using System.Globalization;
using System.Windows.Input;
using PostIt_Prototype_1.ModelView.PostItObjects;
using PostIt_Prototype_1.PostItObjects;

namespace PostIt_Prototype_1.TimelineControllers
{
    public enum TypeOfChange
    {
        Add, Delete, Restore, Update, Duplicate, Color
    }
    public class TimelineChange
    {
        TypeOfChange _changeType;

        public TypeOfChange ChangeType
        {
            get { return _changeType; }
            set { _changeType = value; }
        }
        int _changedIdeaId;

        public int ChangedIdeaId
        {
            get { return _changedIdeaId; }
            set { _changedIdeaId = value; }
        }
        object _metaData;

        public object MetaData
        {
            get { return _metaData; }
            set { _metaData = value; }
        }
        public string GetChangeTypeString()
        {
            switch (_changeType)
            {
                case TypeOfChange.Add:
                    return "ADD";
                case TypeOfChange.Update:
                    return "UPDATE";
                case TypeOfChange.Delete:
                    return "DELETE";
                case TypeOfChange.Restore:
                    return "RESTORE";
                case TypeOfChange.Duplicate:
                    return "DUPLICATE";
                case TypeOfChange.Color:
                    return "COLOR";
            }
            return string.Empty;
        }
        public TimelineChange()
        {

        }
        public TimelineChange(TypeOfChange changeType, int targetNoteId, object changedData)
        {
            _changeType = changeType;
            _changedIdeaId = targetNoteId;
            _metaData = changedData;
        }
        public static string GetUpdateTypeString(object updateMetaData)
        {
            if(updateMetaData is System.Windows.Point)
            {
                return "POS";
            }
            return "CONTENT";
        }
        public XmlElement ToXml(XmlElement parentNode)
        {
            switch (_changeType)
            {
                case TypeOfChange.Add:
                    return GetAddCommandXml(this, parentNode);
                case TypeOfChange.Delete:
                    return GetDeleteCommandXml(this, parentNode);
                case TypeOfChange.Restore:
                    return GetRestoreCommandXml(this, parentNode);
                case TypeOfChange.Update:
                    return GetUpdateCommandXml(this, parentNode);
                case TypeOfChange.Color:
                    return GetColorCommandXml(this, parentNode);
                case TypeOfChange.Duplicate:
                    return GetDuplicateCommandXml(this, parentNode);
            }
            return null;
        }
        static XmlElement GetAddCommandXml(TimelineChange change, XmlElement parentNode)
        {
            /*
             * Xml node structure
             * <ADD NoteID=" " ContentType="[BITMAP/STROKE/GROUP]" Content=" "/>
             */
            
            try
            {
                var node = parentNode.OwnerDocument.CreateElement(change.GetChangeTypeString());
                var ideaIdAttr = parentNode.OwnerDocument.CreateAttribute("NoteID");
                ideaIdAttr.Value = change.ChangedIdeaId.ToString();
                node.Attributes.Append(ideaIdAttr);
                var ideaContentTypeAttr = parentNode.OwnerDocument.CreateAttribute("ContentType");
                var ideaContentAttr = parentNode.OwnerDocument.CreateAttribute("Content");
                if (change.MetaData is Bitmap)
                {
                    ideaContentTypeAttr.Value = "BITMAP";
                    ideaContentAttr.Value = PostItNote.GetDatatStringOfIdeaContent(change.MetaData);
                }
                else if (change.MetaData is List<int>)
                {
                    ideaContentAttr.Value = "GROUP";
                }
                else if (change.MetaData is StrokeData)
                {

                    ideaContentTypeAttr.Value = "STROKE";
                    ideaContentAttr.Value = (change.MetaData as StrokeData).GetStringFromStrokePoints();
                    var isErasingAttr = parentNode.OwnerDocument.CreateAttribute("IsErasing");
                    isErasingAttr.Value = (change.MetaData as StrokeData).GetStringOfIsErasingAttribute();
                    node.Attributes.Append(isErasingAttr);
                    var strokeColorAttr = parentNode.OwnerDocument.CreateAttribute("Color");
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
        static XmlElement GetDeleteCommandXml(TimelineChange change, XmlElement parentNode)
        {
            /*
             * Xml node structure
             * <DELETE NoteID=" "/>
             */
            try
            {
                var node = parentNode.OwnerDocument.CreateElement(change.GetChangeTypeString());
                var ideaIdAttr = parentNode.OwnerDocument.CreateAttribute("NoteID");
                ideaIdAttr.Value = change.ChangedIdeaId.ToString();
                node.Attributes.Append(ideaIdAttr);
                return node;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static XmlElement GetRestoreCommandXml(TimelineChange change, XmlElement parentNode)
        {
            /*
             * Xml node structure
             * <RESTORE NoteID=" "/>
             */
            try
            {
                var node = parentNode.OwnerDocument.CreateElement(change.GetChangeTypeString());
                var ideaIdAttr = parentNode.OwnerDocument.CreateAttribute("NoteID");
                ideaIdAttr.Value = change.ChangedIdeaId.ToString();
                node.Attributes.Append(ideaIdAttr);
                return node;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static XmlElement GetDuplicateCommandXml(TimelineChange change, XmlElement parentNode)
        {
            /*
             * Xml structure
             * <DUPLICATE RefID=" "/>
             */
            try
            {
                var node = parentNode.OwnerDocument.CreateElement(change.GetChangeTypeString());
                var refIdAttr = parentNode.OwnerDocument.CreateAttribute("RefID");
                refIdAttr.Value = change.ChangedIdeaId.ToString();
                node.Attributes.Append(refIdAttr);
                return node;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static XmlElement GetUpdateCommandXml(TimelineChange change, XmlElement parentNode)
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
                var node = parentNode.OwnerDocument.CreateElement(change.GetChangeTypeString());
                var ideaIdAttr = parentNode.OwnerDocument.CreateAttribute("NoteID");
                ideaIdAttr.Value = change.ChangedIdeaId.ToString();
                node.Attributes.Append(ideaIdAttr);
                var updateTypeAttr = parentNode.OwnerDocument.CreateAttribute("UpdateType");
                updateTypeAttr.Value = GetUpdateTypeString(change.MetaData);
                node.Attributes.Append(updateTypeAttr);
                if (updateTypeAttr.Value.CompareTo("POS") == 0)
                {
                    var newPos = (System.Windows.Point)change.MetaData;
                    var posXAttr = parentNode.OwnerDocument.CreateAttribute("X");
                    posXAttr.Value = newPos.X.ToString();
                    node.Attributes.Append(posXAttr);

                    var posYAttr = parentNode.OwnerDocument.CreateAttribute("Y");
                    posYAttr.Value = newPos.Y.ToString();
                    node.Attributes.Append(posYAttr);
                }
                else if (updateTypeAttr.Value.CompareTo("CONTENT") == 0)
                {
                    var contentAttr = parentNode.OwnerDocument.CreateAttribute("Content");
                    contentAttr.Value = PostItNote.GetDatatStringOfIdeaContent(change.MetaData);
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
        static XmlElement GetColorCommandXml(TimelineChange change, XmlElement parentNode)
        {
            /*
             * Xml node structure
             * <COLOR NoteID=" "  Color=" "/>
             */
            try
            {
                var node = parentNode.OwnerDocument.CreateElement(change.GetChangeTypeString());
                var ideaIdAttr = parentNode.OwnerDocument.CreateAttribute("NoteID");
                ideaIdAttr.Value = change.ChangedIdeaId.ToString();
                node.Attributes.Append(ideaIdAttr);
                var colorAttr = parentNode.OwnerDocument.CreateAttribute("Color");
                colorAttr.Value = (string)change.MetaData;
                node.Attributes.Append(colorAttr);
                return node;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        public static TimelineChange ExtractTimelineChangeFromXmlNode(XmlElement node)
        {
            
            if (node.Name.CompareTo("ADD") == 0)
            {
                return ExtractAddCommandFromXmlNode(node);
            }
            if (node.Name.CompareTo("DELETE") == 0)
            {
                return ExtractDeleteCommandFromXmlNode(node);
            }
            if (node.Name.CompareTo("RESTORE") == 0)
            {
                return ExtractDeleteCommandFromXmlNode(node);
            }
            if (node.Name.CompareTo("UPDATE") == 0)
            {
                return ExtractUpdateCommandFromXmlNode(node);
            }
            if (node.Name.CompareTo("DUPLICATE") == 0)
            {
                return ExtractDuplicateCommandFromXmlNode(node);
            }
            if (node.Name.CompareTo("COLOR") == 0)
            {
                return ExtractColorCommandFromXmlNode(node);
            }
            return null;
        }
        static TimelineChange ExtractAddCommandFromXmlNode(XmlElement node)
        {
            try
            {
                var adDchange = new TimelineChange();
                adDchange.ChangeType = TypeOfChange.Add;
                adDchange.ChangedIdeaId = Int32.Parse(node.Attributes["NoteID"].Value, CultureInfo.InvariantCulture);
                var contentType = node.Attributes["ContentType"].Value;
                var contentStr = node.Attributes["Content"].Value;
                if (contentStr.CompareTo("BITMAP") == 0)
                {
                    var contentBytes = Convert.FromBase64String(contentStr);
                    adDchange.MetaData = Utilities.UtilitiesLib.BytesToBitmap(contentBytes);
                }
                else if (contentStr.CompareTo("STROKE") == 0)
                {
                    //ADDchange.MetaData = PostItObjects.StrokeBasedIdea.ParseContentFromString(contentStr);
                    var strokeData = new StrokeData();
                    strokeData.ParseStrokePointsFromString(contentStr);
                    if (node.HasAttribute("IsErasing"))
                    {
                        strokeData.ParseIsErasingFromString(node.Attributes["IsErasing"].Value);
                    }
                    if (node.HasAttribute("Color"))
                    {
                        strokeData.StrokeColorCode = node.Attributes["Color"].Value;
                    }
                    adDchange.MetaData = strokeData;
                }
                else if (contentStr.CompareTo("GROUP") == 0)
                {

                }
                return adDchange;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static TimelineChange ExtractDeleteCommandFromXmlNode(XmlElement node)
        {
            try
            {
                var deLchange = new TimelineChange();
                deLchange.ChangeType = TypeOfChange.Delete;
                deLchange.ChangedIdeaId = Int32.Parse(node.Attributes["NoteID"].Value, CultureInfo.InvariantCulture);
                return deLchange;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static TimelineChange ExtractRestoreCommandFromXmlNode(XmlElement node)
        {
            try
            {
                var deLchange = new TimelineChange();
                deLchange.ChangeType = TypeOfChange.Restore;
                deLchange.ChangedIdeaId = Int32.Parse(node.Attributes["NoteID"].Value, CultureInfo.InvariantCulture);
                return deLchange;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static TimelineChange ExtractDuplicateCommandFromXmlNode(XmlElement node)
        {
            try
            {
                var dupLchange = new TimelineChange();
                dupLchange.ChangeType = TypeOfChange.Duplicate;
                dupLchange.ChangedIdeaId = Int32.Parse(node.Attributes["RefID"].Value, CultureInfo.InvariantCulture);
                return dupLchange;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
        static TimelineChange ExtractUpdateCommandFromXmlNode(XmlElement node)
        {
            try
            {
                var updatEchange = new TimelineChange();
                updatEchange.ChangeType = TypeOfChange.Update;
                updatEchange.ChangedIdeaId = Int32.Parse(node.Attributes["NoteID"].Value, CultureInfo.InvariantCulture);
                var updateType = node.Attributes["UpdateType"].Value;
                if (updateType.CompareTo("POS") == 0)
                {
                    var x = float.Parse(node.Attributes["X"].Value, CultureInfo.InvariantCulture);
                    var y = float.Parse(node.Attributes["Y"].Value, CultureInfo.InvariantCulture);
                    var p = new System.Windows.Point(x, y);
                    updatEchange.MetaData = p;
                }
                else if (updateType.CompareTo("CONTENT") == 0)
                {
                    var contentStr = node.Attributes["Content"].Value;
                    var contentBytes = Convert.FromBase64String(contentStr);
                    updatEchange.MetaData = Utilities.UtilitiesLib.BytesToBitmap(contentBytes);
                }
                return updatEchange;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;   
        }
        static TimelineChange ExtractColorCommandFromXmlNode(XmlElement node)
        {
            try
            {
                var coloRchange = new TimelineChange();
                coloRchange.ChangeType = TypeOfChange.Delete;
                coloRchange.ChangedIdeaId = Int32.Parse(node.Attributes["NoteID"].Value, CultureInfo.InvariantCulture);
                coloRchange.MetaData = node.Attributes["Color"].Value;
                return coloRchange;
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            return null;
        }
    }
}
