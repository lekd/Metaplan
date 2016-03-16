package com.example.mercobrainstorm.presentation;

import com.example.mercobrainstorm.R;

import android.app.Activity;
import android.app.Dialog;
import android.app.DialogFragment;
import android.content.Context;
import android.os.Bundle;
import android.view.ViewGroup.LayoutParams;
import android.view.Window;
import android.widget.RelativeLayout;

public class NoteEditingDialog extends Dialog{
	public NoteEditingDialog(Context context) {
		super(context);
		// TODO Auto-generated constructor stub
	}
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_NO_TITLE);
		setContentView(R.layout.note_editing_layout);
		
		getWindow().setLayout(500, 500);
	}
}
