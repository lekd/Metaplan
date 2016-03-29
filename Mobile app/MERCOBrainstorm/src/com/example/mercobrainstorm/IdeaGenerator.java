package com.example.mercobrainstorm;

import java.io.File;

import com.example.mercobrainstorm.networking.DropboxV2Helper;
import com.example.mercobrainstorm.networking.DropboxV2NoteUploader;
import com.example.mercobrainstorm.networking.WifiCommunicator;
import com.example.mercobrainstorm.presentation.NoteEditingDialog;
import com.example.mercobrainstorm.presentation.NoteEditingDialog.INoteEditDialogSubmitClickedListener;
import com.example.mercobrainstorm.presentation.SimpleStickyNote;
import com.example.mercobrainstorm.presentation.StickyNote;
import com.example.mercobrainstorm.presentation.SimpleStickyNote.ISimpleNoteEditListener;
import com.example.mercobrainstorm.presentation.StickyNote.INoteContentSubmittedListener;
import com.example.mercobrainstorm.utilities.CommandGenerator;
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
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.RelativeLayout;

public class IdeaGenerator extends Activity implements INoteEditDialogSubmitClickedListener,ISimpleNoteEditListener{
	
	WifiCommunicator wifiCommunicator;
	
	
	
	RelativeLayout noteContainer;
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.idea_generator_panel);
		noteContainer = (RelativeLayout)findViewById(R.id.noteContainer);

		//wifiCommunicator = new WifiCommunicator("192.168.10.2", 2015);
		//wifiCommunicator.start();
		DropboxV2Helper.Init();
		
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
		//wifiCommunicator.stop();
	}
	public void btnAddNoteClicked(View v){
		int containerW = noteContainer.getWidth();
		int containerH = noteContainer.getHeight();
		/*StickyNote note = new StickyNote(this);
		RelativeLayout.LayoutParams noteLayoutParams = new RelativeLayout.LayoutParams(500, 650);
		note.setLayoutParams(noteLayoutParams);
		note.setX(containerW/2 - noteLayoutParams.width/2);
		note.setY(containerH/2 - noteLayoutParams.height/2);
		note.setNoteContentSubmissionListener(this);
		noteContainer.addView(note);
		Button btnAdd = (Button)findViewById(R.id.btn_AddNote);
		btnAdd.bringToFront();
		noteContainer.invalidate();
		*/
		/*SimpleStickyNote note = new SimpleStickyNote(this);
		RelativeLayout.LayoutParams noteLayoutParams = new RelativeLayout.LayoutParams(500, 650);
		note.setLayoutParams(noteLayoutParams);
		
		note.setX(containerW/2 - noteLayoutParams.width/2);
		note.setY(containerH/2 - noteLayoutParams.height/2);
		noteContainer.addView(note);
		note.initControls(this);
		Button btnAdd = (Button)findViewById(R.id.btn_AddNote);
		btnAdd.bringToFront();
		noteContainer.invalidate();*/
		NoteEditingDialog dlg = new NoteEditingDialog(this);
		dlg.setIsOnTabletMode(getResources().getBoolean(R.bool.isOnTablet));
		dlg.setNoteContentSubmittedEventListener(this);
		dlg.show();
	}
	public void btnOpenBoardClicked(View v){
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
		}
		else{
			Bitmap transparentNoteContent = Utilities.createTransparentBitmapFromBitmap(noteBmp, Color.WHITE);
			noteOnBoard.setContent(transparentNoteContent);
			noteOnBoard.setOriginData(note.getContentAsPoints());
		}
		
		String noteStoredFileName = String.valueOf(noteOnBoard.getGlobalID()) + ".png";
		
		File noteImageFile = Utilities.Bitmap2File(this, noteBmp,noteStoredFileName);
		DropboxV2NoteUploader uploader = new DropboxV2NoteUploader(this, DropboxV2Helper.getDbxClient());
		Object[] uploadParams = new Object[2];
		uploadParams[0] = noteImageFile;
		uploadParams[1] = noteStoredFileName;
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
