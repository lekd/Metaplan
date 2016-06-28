package com.example.mercobrainstorm.networking;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.concurrent.locks.ReentrantLock;

import org.json.JSONException;
import org.json.JSONObject;

import com.dropbox.core.DbxException;
import com.dropbox.core.v2.DbxClientV2;
import com.dropbox.core.v2.DbxFiles.GetMetadataException;
import com.dropbox.core.v2.DbxFiles.Metadata;

import android.content.Context;
import android.os.AsyncTask;
import android.util.Log;

public class DropboxV2Downloader extends AsyncTask<Object, Integer, Boolean>{

	private DbxClientV2 dbxClient = null;
	private Context mContext = null;
	private FileOutputStream downloadedFile = null;
	private String mPath;
	private String updateTimeStr = "";
    IScreenshotDownloadFinishListener downloadFinishListener = null;
    public void setDownloadFinishListener(IScreenshotDownloadFinishListener listener){
		downloadFinishListener = listener;
	}
	public DropboxV2Downloader(DbxClientV2 client,Context ctx){
		dbxClient = client;
		mContext = ctx;
	}
	public boolean checkIfFileUpdated(String latestModTimeStr,String targetFolder,String targetFileName){
		String fullFilePathOnCloud = targetFolder + "/" + targetFileName;
		try {
			Metadata fileMetaData = dbxClient.files.getMetadata(fullFilePathOnCloud);
			JSONObject mainObject = new JSONObject(fileMetaData.toJson(true));
			String modDateTimeStr = mainObject.getString("server_modified");
			SimpleDateFormat  format = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'");
			if(latestModTimeStr == ""){
				updateTimeStr = modDateTimeStr;
				return true;
			}
			Date modDateTime = format.parse(modDateTimeStr);
			Date latestModTime = format.parse(latestModTimeStr);
			if(modDateTime.after(latestModTime)){
				updateTimeStr = modDateTimeStr;
				return true;
			}
		} catch (GetMetadataException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (DbxException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (JSONException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (ParseException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		return false;
	}
	@Override
	protected Boolean doInBackground(Object... params) {
		// TODO Auto-generated method stub
		String targetFolder = (String)params[0];
		String targetFileName = (String)params[1];
		String latestUpdateTimeStr = (String)params[2];
		//mPath = mContext.getCacheDir().getAbsolutePath() + "/" + targetFileName;
		mPath = new File(mContext.getCacheDir(),targetFileName).getAbsolutePath();
		//mPath = "file:///android_asset/" + targetFileName;
		String fullFilePathOnCloud = targetFolder + "/" + targetFileName;
		try {
			downloadedFile = new FileOutputStream(mPath);
			if(checkIfFileUpdated(latestUpdateTimeStr,targetFolder,targetFileName)){
				dbxClient.files.downloadBuilder(fullFilePathOnCloud).run(downloadedFile);
			}
			else{
				downloadedFile = null;
			}
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
			downloadFinishListener.screenshotDownloadedEventHandler(downloadedFile, mPath, updateTimeStr);
		}
	}
	public interface IScreenshotDownloadFinishListener{
		void screenshotDownloadedEventHandler(FileOutputStream downloadedFile,String screenshotPath, String newUpdateTimeStr);
		
	}
}
