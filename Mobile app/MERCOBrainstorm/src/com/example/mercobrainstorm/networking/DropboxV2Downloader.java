package com.example.mercobrainstorm.networking;

import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;

import com.dropbox.core.DbxException;
import com.dropbox.core.v2.DbxClientV2;

import android.content.Context;
import android.os.AsyncTask;

public class DropboxV2Downloader extends AsyncTask<String, Integer, Boolean>{

	private DbxClientV2 dbxClient = null;
	private Context mContext = null;
	private FileOutputStream downloadedFile = null;
	private String mPath;
    IScreenshotDownloadFinishListener downloadFinishListener = null;
    public void setDownloadFinishListener(IScreenshotDownloadFinishListener listener){
		downloadFinishListener = listener;
	}
	public DropboxV2Downloader(DbxClientV2 client,Context ctx){
		dbxClient = client;
		mContext = ctx;
	}
	@Override
	protected Boolean doInBackground(String... params) {
		// TODO Auto-generated method stub
		String targetFolder = params[0];
		String targetFileName = params[1];
		mPath = mContext.getCacheDir().getAbsolutePath() + "/" + targetFileName; 
		String fullFilePathOnCloud = targetFolder + "/" + targetFileName;
		try {
			downloadedFile = new FileOutputStream(mPath);
			dbxClient.files.downloadBuilder(fullFilePathOnCloud).run(downloadedFile);
		} catch (FileNotFoundException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (DbxException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		return true;
	}
	@Override
    protected void onPostExecute(Boolean result) {
		if (result && downloadFinishListener!=null){
			downloadFinishListener.screenshotDownloadedEventHandler(downloadedFile, mPath);
		}
	}
	public interface IScreenshotDownloadFinishListener{
		void screenshotDownloadedEventHandler(FileOutputStream downloadedFile,String screenshotPath);
		
	}
}
