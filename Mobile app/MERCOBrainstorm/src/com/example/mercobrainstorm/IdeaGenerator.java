package com.example.mercobrainstorm;

import java.io.File;

import com.example.mercobrainstorm.networking.DropboxV2Helper;
import com.example.mercobrainstorm.networking.DropboxV2Uploader;
import com.example.mercobrainstorm.presentation.NoteEditingDialog;
import com.example.mercobrainstorm.presentation.NoteEditingDialog.INoteEditDialogSubmitClickedListener;
import com.example.mercobrainstorm.presentation.SimpleStickyNote;
import com.example.mercobrainstorm.presentation.StickyNote;
import com.example.mercobrainstorm.presentation.SimpleStickyNote.ISimpleNoteEditListener;
import com.example.mercobrainstorm.presentation.StickyNote.INoteContentSubmittedListener;
import com.example.mercobrainstorm.utilities.BrainstormLogger;
import com.example.mercobrainstorm.utilities.CommandGenerator;
import com.example.mercobrainstorm.utilities.IDGenerator;
import com.example.mercobrainstorm.utilities.Utilities;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.Color;
import android.os.Bundle;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.RelativeLayout;

public class IdeaGenerator extends Activity implements INoteEditDialogSubmitClickedListener,ISimpleNoteEditListener{
	
	RelativeLayout noteContainer;
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.idea_generator_panel);
		noteContainer = (RelativeLayout)findViewById(R.id.noteContainer);

		//wifiCommunicator = new WifiCommunicator("192.168.10.2", 2015);
		//wifiCommunicator.start();
		DropboxV2Helper.Init();
		BrainstormLogger.initLogFileName(IDGenerator.getDeviceID(), this);
		BrainstormLogger brainstormLogger = new BrainstormLogger();
		Object[] logParams = new Object[1];
		logParams[0] = BrainstormLogger.getLogStr_Start();
		brainstormLogger.execute(logParams);
	}
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		// Handle action bar item clicks here. The action bar will
		// automatically handle clicks on the Home/Up button, so long
		// as you specify a parent activity in AndroidManifest.xml.
		int id = item.getItemId();
		if (id == R.id.action_settings) {
			return true;
		}
		if (id == R.id.menu_item_sync_log){
			BrainstormLogger brainstormLogger = new BrainstormLogger();
			Object[] logParams = new Object[1];
			logParams[0] = BrainstormLogger.getLogStr_End();
			brainstormLogger.execute(logParams);
			try {
				Thread.sleep(200);
				DropboxV2Uploader uploader = new DropboxV2Uploader(this, DropboxV2Helper.getDbxClient());
				Object[] uploadParams = new Object[4];
				uploadParams[0] = new File(this.getCacheDir(),BrainstormLogger.getLogFileName());
				uploadParams[1] = "/UserStudy_Log/";
				uploadParams[2] = BrainstormLogger.getLogFileName();
				uploadParams[3] = true;
				uploader.execute(uploadParams);
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
		return super.onOptionsItemSelected(item);
	}
	@Override
	public void onStart(){
		super.onStart();
	}
	@Override
    protected void onResume(){
    	super.onResume();
    	BrainstormLogger brainstormLogger = new BrainstormLogger();
		Object[] logParams = new Object[1];
		logParams[0] = BrainstormLogger.getLogStr_Resumed("IdeaGenerator");
		brainstormLogger.execute(logParams);
    }

    @Override
	public void onBackPressed() {
		//onPause();
	}
    @Override
	public void onPause() {
		super.onPause();
		BrainstormLogger brainstormLogger = new BrainstormLogger();
		Object[] logParams = new Object[1];
		logParams[0] = BrainstormLogger.getLogStr_Paused("IdeaGenerator");
		brainstormLogger.execute(logParams);
	}
	@Override
	public void onStop(){
		//dataSender.cancel();
		super.onStop();
		
	}
	@Override
	public void onDestroy(){
		super.onDestroy();
		//wifiCommunicator.stop();
	}
	public void btnAddNoteClicked(View v){
		int containerW = noteContainer.getWidth();
		int containerH = noteContainer.getHeight();
		NoteEditingDialog dlg = new NoteEditingDialog(this);
		dlg.setIsOnTabletMode(getResources().getBoolean(R.bool.isOnTablet));
		dlg.setNoteContentSubmittedEventListener(this);
		dlg.show();
	}
	public void btnOpenBoardClicked(View v){
		BrainstormLogger brainstormLogger = new BrainstormLogger();
		Object[] logParams = new Object[1];
		logParams[0] = BrainstormLogger.getLogStr_switch2Surveillance();
		brainstormLogger.execute(logParams);
		Intent intentOpenBoard = new Intent(this,MetaplanBoard.class);
		startActivity(intentOpenBoard);
	}
	@Override
	public void noteEditDialogClickedEvent(Object sender, Object arg) {
		((NoteEditingDialog)sender).dismiss();
		// TODO Auto-generated method stub
		StickyNote note = (StickyNote)arg;
		Bitmap noteBmp = note.getContentAsBitmap();
		SimpleStickyNote noteOnBoard = getNoteWithLocalID(note.getLocalID());
		if(noteOnBoard == null){
			noteOnBoard = new SimpleStickyNote(this);
			noteOnBoard.setNoteEditTriggeredListener(this);
			Bitmap transparentNoteContent = Utilities.createTransparentBitmapFromBitmap(noteBmp, Color.WHITE);
			noteOnBoard.setContent(transparentNoteContent);
			noteOnBoard.setOriginData(note.getContentAsPoints());
			addASimpleNoteToBoard(noteOnBoard);
			
			BrainstormLogger brainstormLogger = new BrainstormLogger();
			Object[] logParams = new Object[1];
			logParams[0] = BrainstormLogger.getLogStr_NoteAdded(IDGenerator.getHashedID(note.getLocalID()));
			brainstormLogger.execute(logParams);
		}
		else{
			Bitmap transparentNoteContent = Utilities.createTransparentBitmapFromBitmap(noteBmp, Color.WHITE);
			noteOnBoard.setContent(transparentNoteContent);
			noteOnBoard.setOriginData(note.getContentAsPoints());
			
			BrainstormLogger brainstormLogger = new BrainstormLogger();
			Object[] logParams = new Object[1];
			logParams[0] = BrainstormLogger.getLogStr_NoteEdited(IDGenerator.getHashedID(note.getLocalID()));
			brainstormLogger.execute(logParams);
		}
		
		String noteStoredFileName = String.valueOf(noteOnBoard.getGlobalID()) + ".png";
		String targetFolder = "/CELTIC_Notes/";
		Boolean notifyWhenUploaded = true;
		
		File noteImageFile = Utilities.Bitmap2File(this, noteBmp,noteStoredFileName);
		DropboxV2Uploader uploader = new DropboxV2Uploader(this, DropboxV2Helper.getDbxClient());
		Object[] uploadParams = new Object[4];
		uploadParams[0] = noteImageFile;
		uploadParams[1] = targetFolder;
		uploadParams[2] = noteStoredFileName;
		uploadParams[3] = notifyWhenUploaded;
		uploader.execute(uploadParams);
	}
	@Override
	public void simpleNoteEditEventTriggered(Object sender) {
		// TODO Auto-generated method stub
		SimpleStickyNote sendingNote = (SimpleStickyNote)sender;
		NoteEditingDialog dlg = new NoteEditingDialog(this);
		dlg.setNoteID(sendingNote.getLocalID());
		dlg.setNoteContentPoints(sendingNote.getOriginData());
		dlg.setIsOnTabletMode(getResources().getBoolean(R.bool.isOnTablet));
		dlg.setNoteContentSubmittedEventListener(this);
		dlg.show();
	}
	
	void showConnectCloudDialog(){
		AlertDialog.Builder builder1 = new AlertDialog.Builder(this);
		builder1.setMessage("You are not connected to the cloud. Click the connect button to connect");
		builder1.setPositiveButton("Connect",new DialogInterface.OnClickListener() {
			
			@Override
			public void onClick(DialogInterface dlg, int id) {
				// TODO Auto-generated method stub
				dlg.cancel();
			}
		});
		AlertDialog alert1 = builder1.create();
		alert1.show();
	}
	void addASimpleNoteToBoard(SimpleStickyNote note){
		int containerW = noteContainer.getWidth();
		int containerH = noteContainer.getHeight();
		RelativeLayout.LayoutParams noteLayoutParams = new RelativeLayout.LayoutParams(500, 550);
		note.setLayoutParams(noteLayoutParams);
		
		note.setX(containerW/2 - noteLayoutParams.width/2);
		note.setY(containerH/2 - noteLayoutParams.height/2);
		noteContainer.addView(note);
		note.initControls(this);
		Button btnAdd = (Button)findViewById(R.id.btn_AddNote);
		btnAdd.bringToFront();
		noteContainer.invalidate();
	}
	SimpleStickyNote getNoteWithLocalID(int localID){
		SimpleStickyNote note = null;
		for(int i=0; i<noteContainer.getChildCount();i++){
			View child = noteContainer.getChildAt(i);
			if(child instanceof SimpleStickyNote){
				if(((SimpleStickyNote)child).getLocalID() == localID){
					note = (SimpleStickyNote)child;
				}
			}
		}
		return note;
	}
	
	
}
