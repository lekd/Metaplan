package com.example.mercobrainstorm.utilities;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;

import android.content.Context;
import android.os.AsyncTask;
import android.text.format.DateFormat;
import android.util.Log;

public class BrainstormLogger extends AsyncTask<Object, Integer, Boolean>{

	static boolean isWritingLogFile = false;
	static String logFileName = "";
	static String logFilePath = "";
	static boolean logFileInitiated = false;
	static Context context;
	public BrainstormLogger(){
		
	}
	public static String getLogFileName(){
		return logFileName;
	}
	public static void initLogFileName(int deviceID,Context ctx){
		if(!logFileInitiated){
			SimpleDateFormat df = new SimpleDateFormat("yyyy-MM-dd HH-mm");
			String sdt = df.format(new Date(System.currentTimeMillis()));
			logFileName = String.format("Mobile_%d_%s.csv", deviceID,sdt);
			logFileInitiated = true;
			context = ctx;
			File f = new File(context.getCacheDir(),logFileName);
			if(!f.exists()){
				try {
					f.createNewFile();
					logFilePath = f.getAbsolutePath();
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			}
		}
	}
	@Override
	protected Boolean doInBackground(Object... params) {
		// TODO Auto-generated method stub
		if(isWritingLogFile){
			return true;
		}
		isWritingLogFile = true;
		String logMessage = (String)params[0];
		try{
			File f = new File(context.getCacheDir(),logFileName);
			FileWriter fw = new  FileWriter(f, true);
			fw.append(logMessage);
			fw.flush();
			fw.close();
		}
		catch(Exception ex){
			Log.i("BraintormLog", ex.toString());
		}
		return true;
	}
	@Override
    protected void onPostExecute(Boolean result) {
		isWritingLogFile = false;
	}
	public static String getLogStr_NoteAdded(int globalNoteID){
		String logStr = "";
		SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss.SS");
		String timestamp = df.format(new Date(System.currentTimeMillis()));
		String objectType =  "Note";
		String commandType = "Added";
		logStr = String.format("%s;%d;%s;%s\n", timestamp,globalNoteID,objectType,commandType);
		return logStr;
	}
	public static String getLogStr_NoteEdited(int globalNoteID){
		String logStr = "";
		SimpleDateFormat df = new SimpleDateFormat("HH-mm-ss.SS");
		String timestamp = df.format(new Date(System.currentTimeMillis()));
		String objectType =  "Note";
		String commandType = "Edited";
		logStr = String.format("%s;%d;%s;%s\n", timestamp,globalNoteID,objectType,commandType);
		return logStr;
	}
	public static String getLogStr_NoteMoved(int globalNoteID){
		String logStr = "";
		SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss.SS");
		String timestamp = df.format(new Date(System.currentTimeMillis()));
		String objectType =  "Note";
		String commandType = "Moved";
		logStr = String.format("%s;%d;%s;%s\n", timestamp,globalNoteID,objectType,commandType);
		return logStr;
	}
	public static String getLogStr_PointerDown(int deviceID,float X,float Y){
		String logStr = "";
		SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss.SS");
		String timestamp = df.format(new Date(System.currentTimeMillis()));
		String objectType =  "Pointer";
		String commandType = "Down";
		logStr = String.format("%s;%d;%s;%s;%f;%f\n", timestamp,deviceID,objectType,commandType,X,Y);
		return logStr;
	}
	public static String getLogStr_PointerMove(int deviceID,float X,float Y){
		String logStr = "";
		SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss.SS");
		String timestamp = df.format(new Date(System.currentTimeMillis()));
		String objectType =  "Pointer";
		String commandType = "Move";
		logStr = String.format("%s;%d;%s;%s;%f;%f\n", timestamp,deviceID,objectType,commandType,X,Y);
		return logStr;
	}
	public static String getLogStr_PointerUp(int deviceID){
		String logStr = "";
		SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss.SS");
		String timestamp = df.format(new Date(System.currentTimeMillis()));
		String objectType =  "Pointer";
		String commandType = "Up";
		logStr = String.format("%s;%d;%s;%s\n", timestamp,deviceID,objectType,commandType);
		return logStr;
	}
	public static String getLogStr_Start(){
		String logStr = "";
		SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss.SS");
		String timestamp = df.format(new Date(System.currentTimeMillis()));
		String commandType = "Start";
		logStr = String.format("%s;0;%s\n", timestamp,commandType);
		return logStr;
	}
	public static String getLogStr_End(){
		String logStr = "";
		SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss.SS");
		String timestamp = df.format(new Date(System.currentTimeMillis()));
		String commandType = "End";
		logStr = String.format("%s;0;%s\n", timestamp,commandType);
		return logStr;
	}
	public static String getLogStr_switch2NoteGenerator(){
		String logStr = "";
		SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss.SS");
		String timestamp = df.format(new Date(System.currentTimeMillis()));
		String commandType = "To Note Generator";
		logStr = String.format("%s;0;%s\n", timestamp,commandType);
		return logStr;
	}
	public static String getLogStr_switch2Surveillance(){
		String logStr = "";
		SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss.SS");
		String timestamp = df.format(new Date(System.currentTimeMillis()));
		String commandType = "To Surveillance";
		logStr = String.format("%s;0;%s\n", timestamp,commandType);
		return logStr;
	}
}
