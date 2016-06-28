package com.example.mercobrainstorm;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.Date;
import java.util.Locale;

import com.dropbox.client2.DropboxAPI;
import com.dropbox.core.DbxHost;
import com.dropbox.core.DbxRequestConfig;
import com.dropbox.core.http.OkHttpRequestor;
import com.dropbox.core.v2.DbxClientV2;
import com.dropbox.core.v2.DbxFiles.MoveException;
import com.example.mercobrainstorm.networking.DropboxV2Downloader;
import com.example.mercobrainstorm.networking.DropboxV2Downloader.IScreenshotDownloadFinishListener;
import com.example.mercobrainstorm.networking.P2PCommunicator.IP2PCommunicationEventListener;
import com.example.mercobrainstorm.networking.DropboxV2Helper;
import com.example.mercobrainstorm.networking.DropboxV2Uploader;
import com.example.mercobrainstorm.networking.P2PCommunicator;
import com.example.mercobrainstorm.utilities.BrainstormLogger;
import com.example.mercobrainstorm.utilities.IDGenerator;
import com.example.mercobrainstorm.utilities.Utilities;

import android.app.Activity;
import android.app.AlarmManager;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Matrix;
import android.graphics.Rect;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.os.Handler;
import android.util.Log;
import android.view.Menu;
import android.view.MotionEvent;
import android.view.View;
import android.view.View.OnTouchListener;
import android.view.ViewGroup;
import android.view.Window;
import android.view.WindowManager;
import android.view.animation.AlphaAnimation;
import android.view.animation.Animation;
import android.view.animation.Animation.AnimationListener;
import android.widget.ImageView;
import android.widget.RelativeLayout;


public class MetaplanBoard extends Activity implements IScreenshotDownloadFinishListener,IP2PCommunicationEventListener, OnTouchListener{
	private static String TAG = "METAPLANBOARD";
	Handler scheduler = null;
	boolean keep_updating;
	int DELAY_TO_UPDATE = 100;
	String latestUpdateTimeStr = "";
	ImageView metaplanBoardImage;
	Rect actualScreenshotBoundaries = new Rect();
	ImageView iv_pointer;
	
	P2PCommunicator p2pCommunicator;
	boolean isDownloading = false;
	boolean screenshotSet = false;
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		 requestWindowFeature(Window.FEATURE_NO_TITLE);
		 getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN, 
		                            WindowManager.LayoutParams.FLAG_FULLSCREEN);
		super.onCreate(savedInstanceState);
		setContentView(R.layout.metaplan_board);
		getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
		
		updateScreenshotFromCloud();
		metaplanBoardImage = (ImageView)findViewById(R.id.imv_MetaplanBoard);
		metaplanBoardImage.setOnTouchListener(this);
		
		iv_pointer = (ImageView)findViewById(R.id.imv_pointer);
		
		scheduler = new Handler();
		keep_updating = true;
		scheduler.postDelayed(new Runnable(){

			@Override
			public void run() {
				// TODO Auto-generated method stub
				updateScreenshotFromCloud();
				if(keep_updating){
					scheduler.postDelayed(this, DELAY_TO_UPDATE);
				}
			}
			
		}, DELAY_TO_UPDATE);
		
	
		p2pCommunicator = new P2PCommunicator("139.162.183.218", 3004);
		p2pCommunicator.setP2PEventListener(this);
		p2pCommunicator.start();
	}
	
	
	void updateScreenshotFromCloud(){
		//DropboxAPI<?> dropboxAPI = DropboxSandbox.getDropboxAPI();
		
		//DropboxScreenshotDownloader downloader = new DropboxScreenshotDownloader(this, dropboxAPI);
		//downloader.setDownloadFinishListener(this);
		//downloader.execute();
		if(isDownloading){
			return;
		}
		DropboxV2Downloader downloader = new DropboxV2Downloader(DropboxV2Helper.getDbxClient(), this);
		downloader.setDownloadFinishListener(this);
		Object[] downloadParams = new Object[3];
		downloadParams[0] = "";//target folder
		downloadParams[1] = "MetaplanBoard_CELTIC.png";//target file
		downloadParams[2] = latestUpdateTimeStr;
		isDownloading = true;
		downloader.execute(downloadParams);
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
		p2pCommunicator.Release();
		keep_updating = false;
		Intent i = new Intent(this,IdeaGenerator.class);
		i.setFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
		startActivity(i);
		BrainstormLogger brainstormLogger = new BrainstormLogger();
		Object[] logParams = new Object[1];
		logParams[0] = BrainstormLogger.getLogStr_switch2NoteGenerator();
		brainstormLogger.execute(logParams);
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
	Drawable screenshotDrawable = null;
	@Override
	public void screenshotDownloadedEventHandler(
			FileOutputStream downloadedFile, String screenshotPath, String newUpdateTimeStr) {
		// TODO Auto-generated method stub
		try {
			if(downloadedFile != null){
				downloadedFile.flush();
				downloadedFile.close();
				downloadedFile = null;
				
				screenshotDrawable = Drawable.createFromPath(screenshotPath);
				if(screenshotDrawable != null){
					metaplanBoardImage.setImageDrawable(screenshotDrawable);
					latestUpdateTimeStr = newUpdateTimeStr;
					getRealImageBoundaries(true);
					screenshotSet = true;
				}
			}
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		isDownloading = false;
	}
	
	void getRealImageBoundaries(Boolean includeLayout){
		//Rect r = metaplanBoardImage.getDrawable().getBounds();
		int[] offset = new int[2];
        float[] values = new float[9];

        Matrix m = metaplanBoardImage.getImageMatrix();
        m.getValues(values);

        offset[0] = (int) values[5];
        offset[1] = (int) values[2];

        if (includeLayout) {
            ViewGroup.MarginLayoutParams lp = (ViewGroup.MarginLayoutParams) metaplanBoardImage.getLayoutParams();
            int paddingTop = (int) (metaplanBoardImage.getPaddingTop() );
            int paddingLeft = (int) (metaplanBoardImage.getPaddingLeft() );

            offset[0] += paddingTop + lp.topMargin;
            offset[1] += paddingLeft + lp.leftMargin;
        }
        actualScreenshotBoundaries.left = offset[1];
        actualScreenshotBoundaries.top = offset[0];
        int origW = metaplanBoardImage.getDrawable().getIntrinsicWidth();
        int origH = metaplanBoardImage.getDrawable().getIntrinsicHeight();
        float scaleX = values[Matrix.MSCALE_X];
        float scaleY = values[Matrix.MSCALE_Y];
        actualScreenshotBoundaries.right = actualScreenshotBoundaries.left + Math.round(origW*scaleX);
        actualScreenshotBoundaries.bottom = actualScreenshotBoundaries.top + Math.round(origH*scaleY);
	}
	@Override
	public boolean onTouch(View v, MotionEvent event) {
		// TODO Auto-generated method stub
		if(!screenshotSet){
			return false;
		}
		int action = event.getAction();
		float touchX, touchY;
		float[] relativeXY;
		BrainstormLogger brainstormLogger = new BrainstormLogger();
		Object[] logParams = new Object[1];
		switch(action){
		case MotionEvent.ACTION_DOWN:
			touchX = event.getX();
			touchY = event.getY();
			movePointer(touchX, touchY);
			fadeInPointer();
			relativeXY = calculatePosRelativeToScreenshotBoundaries(touchX, touchY, actualScreenshotBoundaries);
			//syncPointerPositionToCloud(relativeXY[0], relativeXY[1]);
			logParams[0] = BrainstormLogger.getLogStr_PointerDown(IDGenerator.getDeviceID(), relativeXY[0], relativeXY[1]);
			brainstormLogger.execute(logParams);
			sendOverPointerPosition(relativeXY[0], relativeXY[1]);
			break;
		case MotionEvent.ACTION_MOVE:
			touchX = event.getX();
			touchY = event.getY();
			movePointer(touchX, touchY);
			relativeXY = calculatePosRelativeToScreenshotBoundaries(touchX, touchY, actualScreenshotBoundaries);
			//syncPointerPositionToCloud(relativeXY[0], relativeXY[1]);
			logParams[0] = BrainstormLogger.getLogStr_PointerMove(IDGenerator.getDeviceID(), relativeXY[0], relativeXY[1]);
			brainstormLogger.execute(logParams);
			sendOverPointerPosition(relativeXY[0], relativeXY[1]);
			break;
		case MotionEvent.ACTION_UP:
			relativeXY = new float[2];
			relativeXY[0] = -10;
			relativeXY[1] = -10;
			fadeOutPointer();
			logParams[0] = BrainstormLogger.getLogStr_PointerUp(IDGenerator.getDeviceID());
			brainstormLogger.execute(logParams);
			//syncPointerPositionToCloud(relativeXY[0], relativeXY[1]);
			sendOverPointerPosition(relativeXY[0], relativeXY[1]);
			break;
		}
		return true;
	}
	
	float[] calculatePosRelativeToScreenshotBoundaries(float absX,float absY, Rect boundaries){
		float[] relativeXY = new float[2];
		relativeXY[0] = (absX - boundaries.left)/boundaries.width();
		relativeXY[1] = (absY - boundaries.top)/boundaries.height();
		return relativeXY;
	}
	void sendOverPointerPosition(float X, float Y){
		byte[] ID_bytes = Utilities.int2ByteArray(IDGenerator.getDeviceID());
		byte[] X_bytes = Utilities.float2ByteArray(X);
		byte[] Y_bytes = Utilities.float2ByteArray(Y);
		byte[] bytesToSend = new byte[ID_bytes.length + X_bytes.length + Y_bytes.length + 1];
		int index = 0;
		System.arraycopy(ID_bytes, 0, bytesToSend, index, ID_bytes.length);
		index += ID_bytes.length;
		System.arraycopy(X_bytes, 0, bytesToSend, index, X_bytes.length);
		index += X_bytes.length;
		System.arraycopy(Y_bytes, 0, bytesToSend, index, Y_bytes.length);
		index += Y_bytes.length;
		bytesToSend[index] = '\n';
		p2pCommunicator.sendData(bytesToSend);
		//Log.i("TouchPos", String.format("%d:%.3f-%.3f", IDGenerator.getDeviceID(),X,Y));
	}
	void syncPointerPositionToCloud(float X,float Y){
		String localPointerFileName = String.valueOf(IDGenerator.getDeviceID()) + ".txt";
		File localF = new File(this.getCacheDir(),localPointerFileName);
		try {
			localF.createNewFile();
			FileOutputStream fs = new FileOutputStream(localF);
			String dataToWrite =  String.format("%.3f;%.3f", X,Y);
			fs.write(dataToWrite.getBytes());
			fs.flush();
			fs.close();
			String cloudPointerFolder = "/TouchPointers/";
			Boolean notifyWhenUploaded = false;
			DropboxV2Uploader pointerUploader = new DropboxV2Uploader(this, DropboxV2Helper.getDbxClient());
			Object[] uploadParams = new Object[4];
			uploadParams[0] = localF;
			uploadParams[1] = cloudPointerFolder;
			uploadParams[2] = localPointerFileName;
			uploadParams[3] = notifyWhenUploaded;
			pointerUploader.execute(uploadParams);
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}


	@Override
	public void UnknowHostEventListener() {
		// TODO Auto-generated method stub
		p2pCommunicator = new P2PCommunicator("139.162.183.218", 3004);
		p2pCommunicator.setP2PEventListener(this);
		p2pCommunicator.start();
	}


	@Override
	public void DataReceivedEventListener(byte[] data, int bytesRead) {
		// TODO Auto-generated method stub
		
	}
	
	void movePointer(float X, float Y){
		RelativeLayout.LayoutParams pointerLP = (RelativeLayout.LayoutParams)iv_pointer.getLayoutParams();
		pointerLP.leftMargin = (int)(X - iv_pointer.getWidth()/2);
		pointerLP.topMargin = (int)(Y - iv_pointer.getHeight()/2);
		iv_pointer.setLayoutParams(pointerLP);
	}
	void fadeInPointer(){
		Animation anim = new AlphaAnimation(0.0f,0.5f);
		anim.setDuration(50);
		anim.setAnimationListener(new AnimationListener() {
			
			@Override
			public void onAnimationStart(Animation animation) {
				// TODO Auto-generated method stub
				
			}
			
			@Override
			public void onAnimationRepeat(Animation animation) {
				// TODO Auto-generated method stub
				
			}
			
			@Override
			public void onAnimationEnd(Animation animation) {
				// TODO Auto-generated method stub
				iv_pointer.setAlpha(0.5f);
			}
		});
		iv_pointer.startAnimation(anim);
	}
	void fadeOutPointer(){
		Animation anim = new AlphaAnimation(0.5f,0.0f);
		anim.setDuration(1000);
		anim.setAnimationListener(new AnimationListener() {
			
			@Override
			public void onAnimationStart(Animation animation) {
				// TODO Auto-generated method stub
				
			}
			
			@Override
			public void onAnimationRepeat(Animation animation) {
				// TODO Auto-generated method stub
				
			}
			
			@Override
			public void onAnimationEnd(Animation animation) {
				// TODO Auto-generated method stub
				iv_pointer.setAlpha(0f);
			}
		});
		iv_pointer.startAnimation(anim);
	}
}
