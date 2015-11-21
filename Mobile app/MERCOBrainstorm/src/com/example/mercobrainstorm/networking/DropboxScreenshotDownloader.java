package com.example.mercobrainstorm.networking;

import java.io.FileNotFoundException;
import java.io.FileOutputStream;

import com.dropbox.client2.DropboxAPI;
import com.dropbox.client2.ProgressListener;
import com.dropbox.client2.exception.DropboxException;

import android.content.Context;
import android.os.AsyncTask;

public class DropboxScreenshotDownloader extends AsyncTask<Void, Long, Boolean>{
	private static String TAG =  "DOWNLOADER";
	private DropboxAPI<?> mApi;
	private Context mContext;
	private String mPath;
	private FileOutputStream screenshotFile = null;
	IScreenshotDownloadFinishListener downloadFinishListener = null;
	public void setDownloadFinishListener(IScreenshotDownloadFinishListener listener){
		downloadFinishListener = listener;
	}
	public DropboxScreenshotDownloader(Context context, DropboxAPI<?> api){
		mContext = context.getApplicationContext();
		mApi = api;
		mPath = mContext.getCacheDir().getAbsolutePath() + "/MetaplanBoard.png"; 
	}
	@Override
	protected Boolean doInBackground(Void... arg0) {
		// TODO Auto-generated method stub
		try {
			screenshotFile = new FileOutputStream(mPath);
			mApi.getFile("/MetaplanBoard.png",null, screenshotFile, new ProgressListener(){
	            @Override
	            public long progressInterval() {
	                // Update the progress bar every half-second or so
	                return 500;
	            }

	            @Override
	            public void onProgress(long bytes, long total) {
	                
	            }
	        });
			return true;
		} catch (FileNotFoundException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (DropboxException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		return false;
	}
	@Override
    protected void onPostExecute(Boolean result) {
		if (result && downloadFinishListener!=null){
			downloadFinishListener.screenshotDownloadedEventHandler(screenshotFile, mPath);
		}
	}
	public interface IScreenshotDownloadFinishListener{
		void screenshotDownloadedEventHandler(FileOutputStream downloadedFile,String screenshotPath);
		
	}
}
