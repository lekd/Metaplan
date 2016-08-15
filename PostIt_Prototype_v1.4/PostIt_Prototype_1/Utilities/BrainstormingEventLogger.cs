using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using WhiteboardApp.NetworkCommunicator;
using WhiteboardApp.PostItObjects;

namespace WhiteboardApp.Utilities
{
    using CloudFile = FileMetadata;
    public class BrainstormingEventLogger
    {
        private static volatile BrainstormingEventLogger loggerInstance;
        private static object syncRoot = new Object();
        private ICloudFS<CloudFile> Storage;
        private string localFilePath = "";
        private volatile StreamWriter logWriter;
        private string logFileName_Cloud = "";
        public BrainstormingEventLogger(ICloudFS<CloudFile> storage)
        {
            this.Storage = storage;
            logFileName_Cloud = "Whiteboard_" + DateTime.Now.ToString("dd-MM-yy HH-mm-ss") + ".csv";
            var logFileFolder = Environment.CurrentDirectory + "/BrainstormLog";
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
        public async void Close()
        {
            UploadLogString(getLogStr_End());
            logWriter.Close();


            await Storage.UploadFileAsync(localFilePath, await Storage.GetFolderAsync(UserStudyLogFolder));

            File.Delete(localFilePath);
        }
        public static async Task<BrainstormingEventLogger> GetInstance(ICloudFS<CloudFile> storage)
        {
            if (loggerInstance == null)
            {
                // Make sure log folder exists
                try
                {
                    await storage.GetFolderAsync(UserStudyLogFolder);
                }
                catch (Exception)
                {
                    await storage.CreateFolderAsync(UserStudyLogFolder);
                }

                lock (syncRoot)
                {
                    if (loggerInstance == null)
                    {
                        loggerInstance = new BrainstormingEventLogger(storage);                        
                    }
                }
            }
            
            return loggerInstance;
        }

        public static string UserStudyLogFolder = "UserStudy_Log";

        public void UploadLogString(string logStr)
        {
            try
            {

                logWriter.Write($"{logStr}\n");
                //logWriter.Close();
                //ICloudDirectoryEntry targetFolder = Storage.GetFolderAsync("/UserStudy_Log");

                //Storage.UploadFileAsync(localFilePath, targetFolder);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
        public static string getLogStr_Start()
        {
            var logStr = "";
            var startTime = DateTime.Now.ToString("HH:mm:ss.ff");
            const string id = "0";
            var commandType = "Start";
            logStr = $"{startTime};{id};{commandType}";
            return logStr;
        }
        public static string getLogStr_End()
        {
            var logStr = "";
            var startTime = DateTime.Now.ToString("HH:mm:ss.ff");
            var id = "0";
            var commandType = "End";
            logStr = $"{startTime};{id};{commandType}";
            return logStr;
        }
        public static string getLogStr_NoteAdded(GenericIdeationObjects.IdeationUnit idea)
        {
            var logStr = "";
            var addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            var id = idea.Id.ToString();
            var objectType = "Note";
            var commandType = "Added";
            logStr = $"{addedTime};{id};{objectType};{commandType}";
            return logStr;
        }
        public static string getLogStr_NoteDeleted(GenericIdeationObjects.IdeationUnit idea)
        {
            var logStr = "";
            var addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            var id = idea.Id.ToString();
            var objectType = "Note";
            var commandType = "Deleted";
            logStr = $"{addedTime};{id};{objectType};{commandType}";
            return logStr;
        }
        public static string getLogStr_NoteRestored(GenericIdeationObjects.IdeationUnit idea)
        {
            var logStr = "";
            var addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            var id = idea.Id.ToString();
            var objectType = "Note";
            var commandType = "Restored";
            logStr = $"{addedTime};{id};{objectType};{commandType}";
            return logStr;
        }
        public static string getLogStr_NoteMoved(GenericIdeationObjects.IdeationUnit idea)
        {
            var logStr = "";
            var addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            var id = idea.Id.ToString();
            var objectType = "Note";
            var commandType = "Moved";
            logStr = $"{addedTime};{id};{objectType};{commandType};{idea.CenterX};{idea.CenterY}";
            return logStr;
        }
        public static string getLogStr_NoteSizeChanged(GenericIdeationObjects.IdeationUnit idea, float scaleX, float scaleY)
        {
            var logStr = "";
            var addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            var id = idea.Id.ToString();
            var objectType = "Note";
            var commandType = "Size";
            logStr = $"{addedTime};{id};{objectType};{commandType};{scaleX.ToString()};{scaleY.ToString()}";
            return logStr;
        }
        public static string getLogStr_RemotePointerAdded(RemotePointer pointer)
        {
            var logStr = "";
            var addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            var id = pointer.Id.ToString();
            var objectType = "Pointer";
            var commandType = "Added";
            logStr = $"{addedTime};{id};{objectType};{commandType};{pointer.X};{pointer.Y}";
            return logStr;
        }
        public static string getLogStr_RemotePointerMoved(RemotePointer pointer)
        {
            var logStr = "";
            var addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            var id = pointer.Id.ToString();
            var objectType = "Pointer";
            var commandType = "Moved";
            logStr = $"{addedTime};{id};{objectType};{commandType};{pointer.X};{pointer.Y}";
            return logStr;
        }
        public static string getLogStr_RemotePointerLeft(RemotePointer pointer)
        {
            var logStr = "";
            var addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            var id = pointer.Id.ToString();
            var objectType = "Pointer";
            var commandType = "Left";
            logStr = $"{addedTime};{id};{objectType};{commandType}";
            return logStr;
        }
        public static string getLogStr_RemotePointerReentered(RemotePointer pointer)
        {
            var logStr = "";
            var addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            var id = pointer.Id.ToString();
            var objectType = "Pointer";
            var commandType = "Reentered";
            logStr = $"{addedTime};{id};{objectType};{commandType};{pointer.X};{pointer.Y}";
            return logStr;
        }
        public static string getLogStr_TimelineFrameStartRetrieving(int frameID)
        {
            var logStr = "";
            var addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            var id = frameID.ToString();
            var objectType = "Frame";
            var commandType = "StartRetrieving";
            logStr = $"{addedTime};{id};{objectType};{commandType}";
            return logStr;
        }
        public static string getLogStr_TimelineFrameFinishRetrieving()
        {
            var logStr = "";
            var addedTime = DateTime.Now.ToString("HH:mm:ss.ff");
            var objectType = "Frame";
            var commandType = "FinishRetrieving";
            logStr = $"{addedTime};{0};{objectType};{commandType}";
            return logStr;
        }
    }
}
