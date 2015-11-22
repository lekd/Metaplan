package com.example.mercobrainstorm;

import java.io.FileOutputStream;
import java.util.Locale;

import com.dropbox.client2.DropboxAPI;
import com.dropbox.core.DbxHost;
import com.dropbox.core.DbxRequestConfig;
import com.dropbox.core.http.OkHttpRequestor;
import com.dropbox.core.v2.DbxClientV2;
import com.example.mercobrainstorm.networking.DropboxSandbox;
import com.example.mercobrainstorm.networking.DropboxScreenshotDownloader;
import com.example.mercobrainstorm.networking.DropboxScreenshotDownloader.IScreenshotDownloadFinishListener;

import android.app.Activity;
import android.app.AlarmManager;
import android.content.Context;
import android.content.Intent;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.os.Handler;
import android.util.Log;
import android.view.Menu;
import android.widget.ImageView;


public class MetaplanBoard extends Activity implements IScreenshotDownloadFinishListener{
	private static String TAG = "METAPLANBOARD";
	Handler scheduler = null;
	int DELAY_TO_UPDATE = 10000;
	ImageView metaplanBoardImage;
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.metaplan_board);
		updateScreenshot();
		metaplanBoardImage = (ImageView)findViewById(R.id.imv_MetaplanBoard);
		
		scheduler = new Handler();
		scheduler.postDelayed(new Runnable(){

			@Override
			public void run() {
				// TODO Auto-generated method stub
				updateScreenshot();
				scheduler.postDelayed(this, DELAY_TO_UPDATE);
			}
			
		}, DELAY_TO_UPDATE);
	}
	

	void updateScreenshot(){
		DropboxAPI<?> dropboxAPI = DropboxSandbox.getDropboxAPI();
		
		DropboxScreenshotDownloader downloader = new DropboxScreenshotDownloader(this, dropboxAPI);
		downloader.setDownloadFinishListener(this);
		downloader.execute();
	}
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		return true;
	}
	@Override
	public void onStart(){
		super.onStart();
	}
	@Override
    protected void onResume(){
    	super.onResume();
	}
	@Override
	public void onBackPressed() {
		onPause();
		Intent i = new Intent(this,IdeaGenerator.class);
		i.setFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
		startActivity(i);
	}
	@Override
	public void onPause() {
		super.onPause();
	}
	@Override
	public void onStop(){
		//dataSender.cancel();
		super.onStop();
		
	}
	@Override
	public void onDestroy(){
		super.onDestroy();
	}
	@Override
	public void screenshotDownloadedEventHandler(
			FileOutputStream downloadedFile, String screenshotPath) {
		// TODO Auto-generated method stub
		Drawable screenshotDrawble = Drawable.createFromPath(screenshotPath);
		if(screenshotDrawble != null){
			Log.i("SCREENSHOT", screenshotDrawble.toString());
		}
		metaplanBoardImage.setImageDrawable(screenshotDrawble);
	}
}
