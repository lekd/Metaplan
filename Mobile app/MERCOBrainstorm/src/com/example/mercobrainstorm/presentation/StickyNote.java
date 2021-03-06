package com.example.mercobrainstorm.presentation;



import com.example.mercobrainstorm.R;
import com.example.mercobrainstorm.presentation.NoteWritingCanvas.IWritingEventListener;
import com.example.mercobrainstorm.utilities.IDGenerator;
import com.example.mercobrainstorm.utilities.Utilities;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Color;
import android.graphics.Path;
import android.graphics.Point;
import android.graphics.Rect;
import android.util.AttributeSet;
import android.view.Gravity;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;

public class StickyNote extends LinearLayout implements IWritingEventListener{
	
	
	int _localID = -1;
	public void setLocalID(int id){
		_localID = id;
	}
	public int getLocalID(){
		return _localID;
	}
	NoteWritingCanvas writingCanvas;
	Button okButton;
	ImageButton delButton;
	INoteContentSubmittedListener contentSubmittedListener = null;
	StickyNote myself;
	
	
	public void setNoteContentSubmissionListener(INoteContentSubmittedListener listener){
		contentSubmittedListener = listener;
	}
	public StickyNote(Context context) {
		super(context);
		// TODO Auto-generated constructor stub
		initControl(context);
		myself = this;
	}
	public StickyNote(Context context,AttributeSet attrs) {
		super(context);
		// TODO Auto-generated constructor stub
		initControl(context);
		myself = this;
	}
	void initControl(Context ctx){
		this.setBackground(ctx.getResources().getDrawable(R.drawable.sticky_note_background));
		this.setWeightSum(1.0f);
		this.setOrientation(LinearLayout.VERTICAL);
		
		writingCanvas = new NoteWritingCanvas(ctx);
		LinearLayout.LayoutParams writingCanvasLayoutParams = new LinearLayout.LayoutParams(LayoutParams.MATCH_PARENT,0,0.95f);
		writingCanvasLayoutParams.topMargin = 5;
		writingCanvasLayoutParams.leftMargin = 2;
		writingCanvasLayoutParams.rightMargin = 2;
		writingCanvas.setLayoutParams(writingCanvasLayoutParams);
		writingCanvas.setWritingEventListener(this);
		this.addView(writingCanvas);
		
		
		okButton = new Button(ctx);
		okButton.setBackgroundResource(R.drawable.ok_button);
		LinearLayout.LayoutParams buttonLayoutParams = new LinearLayout.LayoutParams(75,75);
		buttonLayoutParams.gravity = Gravity.BOTTOM | Gravity.END;
		okButton.setLayoutParams(buttonLayoutParams);
		okButton.setEnabled(false);
		okButton.setOnClickListener(new View.OnClickListener() {
			
			@Override
			public void onClick(View v) {
				// TODO Auto-generated method stub
				okButton.setEnabled(false);
				if(contentSubmittedListener != null){
					contentSubmittedListener.contentSubmittedEventHandler(myself);
				}
			}
		});
		this.addView(okButton);
	}
	Point previousTouch;
	@Override
	public boolean onTouchEvent(MotionEvent event){
		float eventX = event.getX();
		float eventY = event.getY();
		Point p = new Point((int)eventX, (int)eventY);
		switch(event.getAction()){
		case MotionEvent.ACTION_DOWN:
			this.bringToFront();
			previousTouch = p;
			break;
		case MotionEvent.ACTION_MOVE:
			float dif_x = p.x - previousTouch.x;
			float dif_y = p.y - previousTouch.y;
			this.setX(this.getX() + dif_x);
			this.setY(this.getY() + dif_y);
			previousTouch = p;
			break;
		}
		return true;
	}
	
	public Bitmap getContentAsBitmap(){
		return writingCanvas.getContentAsBitmap(true);
	}
	public Path getContentAsPoints(){
		return writingCanvas.getPathData();
	}
	public void setContentDataPoints(Path points){
		writingCanvas.setExistingPath(points);
	}
	@Override
	public void writingEventHandler(Object sender) {
		// TODO Auto-generated method stub
		okButton.setEnabled(true);
	}
	//not used anymore
	public byte[] getByteData(){
		byte[] contentBytes = Utilities.Bitmap2Bytes(getContentAsBitmap());
		byte[] numberBuffer = null;
		byte[] allDataBytes = new byte[Integer.SIZE/8 + 3*Float.SIZE/8 + Integer.SIZE/8 + 4 + contentBytes.length];
		int index = 0;
		//parse ID
		numberBuffer = Utilities.int2ByteArray(IDGenerator.getHashedID(_localID));
		System.arraycopy(numberBuffer, 0, allDataBytes, index, numberBuffer.length);
		index += numberBuffer.length;
		//parse X
		numberBuffer = Utilities.float2ByteArray(0.5f);
		System.arraycopy(numberBuffer, 0, allDataBytes, index, numberBuffer.length);
		index += numberBuffer.length;
		//parse Y
		numberBuffer = Utilities.float2ByteArray(0.5f);
		System.arraycopy(numberBuffer, 0, allDataBytes, index, numberBuffer.length);
		index += numberBuffer.length;
		//orientation = 0 by default
		numberBuffer = Utilities.float2ByteArray(0);
		System.arraycopy(numberBuffer, 0, allDataBytes, index, numberBuffer.length);
		index += numberBuffer.length;
		//parse content size
		numberBuffer = Utilities.int2ByteArray(contentBytes.length);
		System.arraycopy(numberBuffer, 0, allDataBytes, index, numberBuffer.length);
		index += numberBuffer.length;
		//put content type
		byte[] contentType = new byte[] { (byte)'@', (byte)'B', (byte)'M', (byte)'P' };
		System.arraycopy(contentType, 0, allDataBytes, index, contentType.length);
		index += contentType.length;
		//put content data
		System.arraycopy(contentBytes, 0, allDataBytes, index, contentBytes.length);
		return allDataBytes;
	}
	public interface INoteContentSubmittedListener{
		void contentSubmittedEventHandler(Object sender);
	}
}
