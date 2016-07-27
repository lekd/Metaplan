using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox;
using System.IO;
using PostIt_Prototype_1.PostItObjects;
using System.Threading;
using PostIt_Prototype_1.NetworkCommunicator;

namespace PostIt_Prototype_1.Utilities
{
    public class BrainstormingEventLogger
    {
        private static volatile BrainstormingEventLogger loggerInstance;
        private static object syncRoot = new Object();
        private DropboxFS Storage ;
        private string localFilePath = "";
        private volatile StreamWriter logWriter;
        private string logFileName_Cloud = "";
        public BrainstormingEventLogger(DropboxFS storage)
        {
            this.Storage = storage;
            logFileName_Cloud = "Whiteboard_" + DateTime.Now.ToString("dd-MM-yy HH-mm-ss") + ".csv";
            string logFileFolder = Environment.CurrentDirectory + "/BrainstormLog";
            if (!Directory.Exists(logFileFolder))
            {
                Directory.CreateDirectory(logFileFolder);
            }
            localFilePath = logFileFolder + "/" + logFileName_Cloud;
            if (!File.Exists(localFilePath))
            {
                File.Create(localFilePath).Close();
            }
            logWriter = File.AppendText(localFilePath);
        }
        public void Close()
        {
            UploadLogString(getLogStr_End());
            logWriter.Close();

            ICloudDirectoryEntry targetFolder = Storage.GetFolder("/UserStudy_Log");

            Storage.UploadFile(localFilePath, targetFolder);

            File.Delete(localFilePath);
        }
        public static BrainstormingEventLogger GetInstance(DropboxFS storage)
        {
            if (loggerInstance == null)
            {
                lock (syncRoot)
                {
                    if (loggerInstance == null)
                    {
                        loggerInstance = new BrainstormingEventLogger(storage);
                        
                        ICloudDirectoryEntry targetFolder;
                        try
                        {
                            targetFolder = storage.GetFolder("/UserStudy_Log");
                        }
                        catch (Exception ex)
                        {
                            targetFolder = storage.CreateFolder("/UserStudy_Log");
                        }
                        
                        
                    }
                }
            }
            return loggerInstance;
        }
        public void UploadLogString(string logStr)
        {
            try
            {
                
                logWriter.Write(String.Format("{0}\n", logStr));
                //logWriter.Close();
                //ICloudDirectoryEntry targetFolder = Storage.GetFolder("/UserStudy_Log");
                
                //Storage.UploadFile(localFilePath, targetFolder);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        public static string getLogStr_Start()
        {
            string logStr = "";
            string startTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string id = "0";
            string commandType = "Start";
            logStr = string.Format("{0};{1};{2}", startTime, id, commandType);
            return logStr;
        }
        public static string getLogStr_End()
        {
            string logStr = "";
            string startTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string id = "0";
            string commandType = "End";
            logStr = string.Format("{0};{1};{2}", startTime, id, commandType);
            return logStr;
        }
        public static string getLogStr_NoteAdded(GenericIdeationObjects.IdeationUnit idea)
        {
            string logStr = "";
            string addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string id = idea.Id.ToString();
            string objectType = "Note";
            string commandType = "Added";
            logStr = string.Format("{0};{1};{2};{3}", addedTime, id, objectType, commandType);
            return logStr;
        }
        public static string getLogStr_NoteDeleted(GenericIdeationObjects.IdeationUnit idea)
        {
            string logStr = "";
            string addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string id = idea.Id.ToString();
            string objectType = "Note";
            string commandType = "Deleted";
            logStr = string.Format("{0};{1};{2};{3}", addedTime, id, objectType, commandType);
            return logStr;
        }
        public static string getLogStr_NoteRestored(GenericIdeationObjects.IdeationUnit idea)
        {
            string logStr = "";
            string addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string id = idea.Id.ToString();
            string objectType = "Note";
            string commandType = "Restored";
            logStr = string.Format("{0};{1};{2};{3}", addedTime, id, objectType, commandType);
            return logStr;
        }
        public static string getLogStr_NoteMoved(GenericIdeationObjects.IdeationUnit idea)
        {
            string logStr = "";
            string addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string id = idea.Id.ToString();
            string objectType = "Note";
            string commandType = "Moved";
            logStr = string.Format("{0};{1};{2};{3};{4};{5}", addedTime, id, objectType, commandType,idea.CenterX,idea.CenterY);
            return logStr;
        }
        public static string getLogStr_NoteSizeChanged(GenericIdeationObjects.IdeationUnit idea, float scaleX, float scaleY)
        {
            string logStr = "";
            string addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string id = idea.Id.ToString();
            string objectType = "Note";
            string commandType = "Size";
            logStr = string.Format("{0};{1};{2};{3};{4};{5}",addedTime,id,objectType,commandType,scaleX.ToString(),scaleY.ToString());
            return logStr;
        }
        public static string getLogStr_RemotePointerAdded(RemotePointer pointer)
        {
            string logStr = "";
            string addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string id = pointer.Id.ToString();
            string objectType = "Pointer";
            string commandType = "Added";
            logStr = string.Format("{0};{1};{2};{3};{4};{5}", addedTime, id, objectType, commandType, pointer.X, pointer.Y);
            return logStr;
        }
        public static string getLogStr_RemotePointerMoved(RemotePointer pointer)
        {
            string logStr = "";
            string addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string id = pointer.Id.ToString();
            string objectType = "Pointer";
            string commandType = "Moved";
            logStr = string.Format("{0};{1};{2};{3};{4};{5}", addedTime, id, objectType, commandType, pointer.X, pointer.Y);
            return logStr;
        }
        public static string getLogStr_RemotePointerLeft(RemotePointer pointer)
        {
            string logStr = "";
            string addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string id = pointer.Id.ToString();
            string objectType = "Pointer";
            string commandType = "Left";
            logStr = string.Format("{0};{1};{2};{3}", addedTime, id, objectType, commandType);
            return logStr;
        }
        public static string getLogStr_RemotePointerReentered(RemotePointer pointer)
        {
            string logStr = "";
            string addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string id = pointer.Id.ToString();
            string objectType = "Pointer";
            string commandType = "Reentered";
            logStr = string.Format("{0};{1};{2};{3};{4};{5}", addedTime, id, objectType, commandType, pointer.X, pointer.Y);
            return logStr;
        }
        public static string getLogStr_TimelineFrameStartRetrieving(int frameID)
        {
            string logStr = "";
            string addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string id = frameID.ToString();
            string objectType = "Frame";
            string commandType = "StartRetrieving";
            logStr = string.Format("{0};{1};{2};{3}", addedTime, id, objectType, commandType);
            return logStr;
        }
        public static string getLogStr_TimelineFrameFinishRetrieving()
        {
            string logStr = "";
            string addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            string objectType = "Frame";
            string commandType = "FinishRetrieving";
            logStr = string.Format("{0};{1};{2};{3}", addedTime, 0, objectType, commandType);
            return logStr;
        }
    }
}
