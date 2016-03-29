package com.example.mercobrainstorm.presentation;

import com.example.mercobrainstorm.R;
import com.example.mercobrainstorm.utilities.IDGenerator;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Path;
import android.graphics.Point;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.ImageView.ScaleType;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;

public class SimpleStickyNote extends LinearLayout{
	static int IDcount = 0;
	int _localID;
	Button editBtn;
	ImageView noteContentImg;
	Context context;
	SimpleStickyNote myself;
	
	Path originData = null;
	
	ISimpleNoteEditListener noteEditTriggeredListener = null;
	public void setNoteEditTriggeredListener(ISimpleNoteEditListener listener){
		noteEditTriggeredListener = listener;
	}
	public Path getOriginData(){
		return originData;
	}
	public void setOriginData(Path dataPoints){
		originData = dataPoints;
	}
	public SimpleStickyNote(Context ctx){
		super(ctx);
		context = ctx;
		_localID = (++IDcount);
		noteContentImg = new ImageView(ctx);
		noteContentImg.setScaleType(ScaleType.FIT_XY);
		myself = this;
	}
	public SimpleStickyNote(Context ctx, AttributeSet attrs) {
		super(ctx, attrs);
		// TODO Auto-generated constructor stub
		context = ctx;
		_localID = (++IDcount);
		noteContentImg = new ImageView(ctx);
		noteContentImg.setScaleType(ScaleType.FIT_XY);
		myself = this;
	}
	public int getLocalID(){
		return _localID;
	}
	public int getGlobalID(){
		return IDGenerator.getHashedID(_localID);
	}
	public void initControls(Context ctx){
		this.setBackground(ctx.getResources().getDrawable(R.drawable.sticky_note_background));
		this.setWeightSum(1.0f);
		this.setOrientation(LinearLayout.VERTICAL);
		
		LinearLayout.LayoutParams noteContentImgVParams = new LinearLayout.LayoutParams(LayoutParams.MATCH_PARENT, 0,0.85f);
		noteContentImgVParams.topMargin = 2;
		noteContentImgVParams.leftMargin = 2;
		noteContentImgVParams.rightMargin = 2;
		noteContentImg.setLayoutParams(noteContentImgVParams);
		this.addView(noteContentImg);
		
		RelativeLayout buttonContainer = new RelativeLayout(ctx);
		LinearLayout.LayoutParams btnContainerParams = new LinearLayout.LayoutParams(LayoutParams.MATCH_PARENT, 0,0.15f);
		buttonContainer.setLayoutParams(btnContainerParams);
		this.addView(buttonContainer);
		
		editBtn = new Button(ctx);
		editBtn.setBackgroundResource(R.drawable.edit_btn);
		android.view.ViewGroup.LayoutParams windowLayoutParams = this.getLayoutParams();
		RelativeLayout.LayoutParams editBtnLayoutParams = new RelativeLayout.LayoutParams((int)(windowLayoutParams.height*0.15),LayoutParams.MATCH_PARENT);
		editBtnLayoutParams.addRule(RelativeLayout.ALIGN_PARENT_RIGHT);
		editBtn.setLayoutParams(editBtnLayoutParams);
		buttonContainer.addView(editBtn);
		editBtn.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {
				// TODO Auto-generated method stub
				if(noteEditTriggeredListener != null){
					noteEditTriggeredListener.simpleNoteEditEventTriggered(myself);
				}
			}
		});
	}
	public void setContent(Bitmap contentBmp){
		noteContentImg.setImageBitmap(contentBmp);
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
	
	public interface ISimpleNoteEditListener{
		void simpleNoteEditEventTriggered(Object sender);
	}
}
