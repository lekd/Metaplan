package com.example.mercobrainstorm;

import java.io.File;

import com.example.mercobrainstorm.networking.DropboxV2Helper;
import com.example.mercobrainstorm.networking.DropboxV2NoteUploader;
import com.example.mercobrainstorm.networking.WifiCommunicator;
import com.example.mercobrainstorm.presentation.StickyNote;
import com.example.mercobrainstorm.presentation.StickyNote.INoteContentSubmittedListener;
import com.example.mercobrainstorm.utilities.CommandGenerator;
import com.example.mercobrainstorm.utilities.Utilities;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.RelativeLayout;

public class IdeaGenerator extends Activity implements INoteContentSubmittedListener{
	
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
		StickyNote note = new StickyNote(this);
		RelativeLayout.LayoutParams noteLayoutParams = new RelativeLayout.LayoutParams(500, 650);
		note.setLayoutParams(noteLayoutParams);
		note.setX(containerW/2 - noteLayoutParams.width/2);
		note.setY(containerH/2 - noteLayoutParams.height/2);
		note.setNoteContentSubmissionListener(this);
		noteContainer.addView(note);
		Button btnAdd = (Button)findViewById(R.id.btn_AddNote);
		btnAdd.bringToFront();
		noteContainer.invalidate();
	}
	public void btnOpenBoardClicked(View v){
		Intent intentOpenBoard = new Intent(this,MetaplanBoard.class);
		startActivity(intentOpenBoard);
	}
	@Override
	public void contentSubmittedEventHandler(Object sender) {
		// TODO Auto-generated method stub
		StickyNote note = (StickyNote)sender;
		ImageView contentDisplayer = (ImageView)findViewById(R.id.iv_capturedContent);
		Bitmap noteBmp = note.getContent();
		//wifiCommunicator.sendData(CommandGenerator.GenerateADDCommand(note.getByteData()));
		String noteStoredFileName = String.valueOf(note.getGlobalID()) + ".png";
		//DropboxNoteUploader uploader = new DropboxNoteUploader(this, dropboxSandbox.getDropboxAPI(), "/Notes/", Utilities.Bitmap2File(this, noteBmp,noteStoredFileName));
		//uploader.execute();
		File noteImageFile = Utilities.Bitmap2File(this, noteBmp,noteStoredFileName);
		DropboxV2NoteUploader uploader = new DropboxV2NoteUploader(this, DropboxV2Helper.getDbxClient());
		Object[] uploadParams = new Object[2];
		uploadParams[0] = noteImageFile;
		uploadParams[1] = noteStoredFileName;
		uploader.execute(uploadParams);
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
}
