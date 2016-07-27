using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public interface IGenericCloudFS<out TFile, TFolder>
    {
        TFolder GetFolder(string folderPath);
        TFolder CreateFolder(string folderPath);
        void DownloadFile(string fileName, TFolder folder, Stream stream);
        TFile UploadFile(Stream stream, string fileName, TFolder folder);
        TFile UploadFile(string localFilePath, TFolder targetFolder);
    }

}
