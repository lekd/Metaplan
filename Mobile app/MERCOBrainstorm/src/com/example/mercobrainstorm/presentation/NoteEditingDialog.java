package com.example.mercobrainstorm.presentation;

import com.example.mercobrainstorm.R;
import com.example.mercobrainstorm.presentation.StickyNote.INoteContentSubmittedListener;

import android.app.Activity;
import android.app.Dialog;
import android.app.DialogFragment;
import android.content.Context;
import android.graphics.Path;
import android.graphics.Point;
import android.os.Bundle;
import android.util.Log;
import android.view.Display;
import android.view.ViewGroup.LayoutParams;
import android.view.Window;
import android.widget.RelativeLayout;

public class NoteEditingDialog extends Dialog implements INoteContentSubmittedListener{
	Context callingActivity = null;
	boolean isOnTablet = false;
	StickyNote sketchingNote;
	public void setIsOnTabletMode(boolean isontablet){
		isOnTablet = isontablet;
	}
	INoteEditDialogSubmitClickedListener contentCommittedListener = null;
	public void setNoteContentSubmittedEventListener(INoteEditDialogSubmitClickedListener listener){
		contentCommittedListener = listener;
	}
	public NoteEditingDialog(Context context) {
		super(context);
		// TODO Auto-generated constructor stub
		callingActivity = context;
		sketchingNote = new StickyNote(callingActivity);
		RelativeLayout.LayoutParams noteLayoutParams = new RelativeLayout.LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.MATCH_PARENT);
		sketchingNote.setLayoutParams(noteLayoutParams);
	}
	public void setNoteID(int id){
		sketchingNote.setLocalID(id);
	}
	public void setNoteContentPoints(Path dataPoints){
		sketchingNote.setContentDataPoints(dataPoints);
	}
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_NO_TITLE);
		setContentView(R.layout.note_editing_layout);
		
		
		RelativeLayout mainContainer = (RelativeLayout)findViewById(R.id.noteEditorContainer);
		mainContainer.addView(sketchingNote);;
		sketchingNote.setNoteContentSubmissionListener(this);
		if(!isOnTablet){
			Display display = ((Activity)callingActivity).getWindowManager().getDefaultDisplay();
			Point size = new Point();
			display.getSize(size);
			if(size.x < size.y){
				getWindow().setLayout(size.x, size.x + 100);
			}
			else{
				getWindow().setLayout(size.y - 100, size.y);
			}
		}
		else{
			Display display = ((Activity)callingActivity).getWindowManager().getDefaultDisplay();
			Point size = new Point();
			display.getSize(size);
			if(size.x < size.y){
				getWindow().setLayout(size.x, size.x + 100);
			}
			else{
				getWindow().setLayout(size.y - 100, size.y);
			}
		}
	}
	@Override
	public void contentSubmittedEventHandler(Object sender) {
		// TODO Auto-generated method stub
		if(contentCommittedListener!= null){
			contentCommittedListener.noteEditDialogClickedEvent(this, sender);
		}
	}
	public interface INoteEditDialogSubmitClickedListener{
		void noteEditDialogClickedEvent(Object sender,Object arg);
	}
}
