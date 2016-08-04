using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Drive.v3.Data;
using PostIt_Prototype_1.Utilities;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public interface ICloudFS<TFolder, TFile>
    {
        TFolder CreateFolder(string folderName);
        TFolder CreateFolder(string folderName, TFolder parent);
        Task<TFolder> CreateFolderAsync(string folderName);
        Task<TFolder> CreateFolderAsync(string folderName, TFolder parent);
        Task DownloadFileAsync(TFile file, Stream stream);
        Task<IList<TFile>> GetChildrenAsync(TFolder folder, GoogleMimeType mimeType = null);
        TFolder GetFolder(string folderName);
        TFolder GetFolder(string folderName, TFolder parent);
        Task<TFolder> GetFolderAsync(string folderName);
        Task<TFolder> GetFolderAsync(string folderName, TFolder parent);
        Task<TFile> UploadFileAsync(string localFilePath, TFolder targetFolder);
        Task<TFile> UploadFileAsync(Stream stream, string fileName, TFolder folder);
    }
}