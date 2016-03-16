package com.example.mercobrainstorm.presentation;

import com.example.mercobrainstorm.R;

import android.content.Context;
import android.util.AttributeSet;
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

	public SimpleStickyNote(Context ctx){
		super(ctx);
		context = ctx;
	}
	public SimpleStickyNote(Context ctx, AttributeSet attrs) {
		super(ctx, attrs);
		// TODO Auto-generated constructor stub
		context = ctx;
	}
	public void initControls(Context ctx){
		this.setBackground(ctx.getResources().getDrawable(R.drawable.sticky_note_background));
		this.setWeightSum(1.0f);
		this.setOrientation(LinearLayout.VERTICAL);
		
		noteContentImg = new ImageView(ctx);
		noteContentImg.setScaleType(ScaleType.FIT_XY);
		LinearLayout.LayoutParams noteContentImgVParams = new LinearLayout.LayoutParams(LayoutParams.MATCH_PARENT, 0,0.9f);
		noteContentImg.setLayoutParams(noteContentImgVParams);
		noteContentImg.setBackgroundResource(R.drawable.round_button);
		this.addView(noteContentImg);
		
		RelativeLayout buttonContainer = new RelativeLayout(ctx);
		LinearLayout.LayoutParams btnContainerParams = new LinearLayout.LayoutParams(LayoutParams.MATCH_PARENT, 0,0.1f);
		buttonContainer.setLayoutParams(btnContainerParams);
		this.addView(buttonContainer);
		
		editBtn = new Button(ctx);
		editBtn.setBackgroundResource(R.drawable.edit_btn);
		android.view.ViewGroup.LayoutParams windowLayoutParams = this.getLayoutParams();
		RelativeLayout.LayoutParams editBtnLayoutParams = new RelativeLayout.LayoutParams((int)(windowLayoutParams.height*0.1),LayoutParams.MATCH_PARENT);
		editBtnLayoutParams.addRule(RelativeLayout.ALIGN_PARENT_RIGHT);
		editBtn.setLayoutParams(editBtnLayoutParams);
		buttonContainer.addView(editBtn);
	}
}
