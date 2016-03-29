package com.example.mercobrainstorm.presentation;

import java.util.ArrayList;
import java.util.List;

import org.apache.http.impl.conn.Wire;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.Point;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;

public class NoteWritingCanvas extends View{
	int RESIZED_NOTE_WIDTH = 200;
	int RESIZED_NOTE_HEIGHT = 200;
	
	private Paint paint = new Paint();
	private Path path = new Path();
	public void setExistingPath(Path p){
		path = p;
	}
	IWritingEventListener writingListener = null;
	public void setWritingEventListener(IWritingEventListener listener){
		writingListener = listener;
	}
	public NoteWritingCanvas(Context context) {
		super(context);
		// TODO Auto-generated constructor stub
		this.setBackgroundColor(Color.YELLOW);
		initializePaint();
	}
	public NoteWritingCanvas(Context context, AttributeSet attrs) {
		super(context);
		// TODO Auto-generated constructor stub
		this.setBackgroundColor(Color.YELLOW);
		initializePaint();
	}
	void initializePaint(){
		paint.setAntiAlias(true);
		paint.setStrokeWidth(3f);
		paint.setColor(Color.BLACK);
		paint.setStyle(Paint.Style.STROKE);
		paint.setStrokeJoin(Paint.Join.ROUND);
	}
	@Override
	protected void onDraw(Canvas canvas){
		canvas.drawPath(path, paint);
	}
	@Override
	public boolean onTouchEvent(MotionEvent event){
		float eventX = event.getX();
		float eventY = event.getY();
		Point p = new Point((int)eventX, (int)eventY);
		switch(event.getAction()){
		case MotionEvent.ACTION_DOWN:
			path.moveTo(eventX, eventY);
			if(writingListener != null){
				writingListener.writingEventHandler(this);
			}
			break;
		case MotionEvent.ACTION_MOVE:
			path.lineTo(eventX, eventY);
			break;
		case MotionEvent.ACTION_UP:
			break;
		}
		invalidate();
		return true;
	}
	public Bitmap getContentAsBitmap(boolean resize){
		Bitmap.Config config = Bitmap.Config.ARGB_8888;
		Bitmap bmp = Bitmap.createBitmap(this.getWidth(), this.getHeight(), config);
		Canvas drawCanvas = new Canvas(bmp);
		drawCanvas.drawColor(Color.TRANSPARENT);
		drawCanvas.drawPath(path, paint);
		if(!resize){
			return bmp;
		}
		Bitmap resizedBmp = Bitmap.createScaledBitmap(bmp, RESIZED_NOTE_WIDTH, RESIZED_NOTE_HEIGHT, false);
		return resizedBmp;
	}
	public Path getPathData(){
		return path;
	}
	public interface IWritingEventListener{
		void writingEventHandler(Object sender);
	}
}
