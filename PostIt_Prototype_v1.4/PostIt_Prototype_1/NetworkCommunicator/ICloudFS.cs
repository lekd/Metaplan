using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Drive.v3.Data;
using WhiteboardApp.Utilities;

namespace WhiteboardApp.NetworkCommunicator
{
    public interface ICloudFS<TFile>
    {
        TFile CreateFolder(string folderName);
        TFile CreateFolder(string folderName, TFile parent);
        Task<TFile> CreateFolderAsync(string folderName);
        Task<TFile> CreateFolderAsync(string folderName, TFile parent);
        Task DownloadFileAsync(TFile file, Stream stream);
        Task<IList<TFile>> GetChildrenAsync(TFile folder, GoogleMimeType mimeType = null);
        TFile GetFolder(string folderName);
        TFile GetFolder(string folderName, TFile parent);
        Task<TFile> GetFolderAsync(string folderName);
        Task<TFile> GetFolderAsync(string folderName, TFile parent);
        Task<TFile> UploadFileAsync(string localFilePath, TFile targetFolder);
        Task<TFile> UploadFileAsync(Stream stream, string fileName, TFile folder);
    }
}