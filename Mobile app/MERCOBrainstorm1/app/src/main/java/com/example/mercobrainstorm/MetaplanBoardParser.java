package com.example.mercobrainstorm;

import java.io.UnsupportedEncodingException;

import android.util.Log;
import android.util.Xml.Encoding;

public class MetaplanBoardParser {
	StringBuilder strBuilder = new StringBuilder();
	String prefixStr;
	String postfixStr;
	public MetaplanBoardParser(){
		byte[] packagePrefix =  new byte[]{'<','W','B','_','S','C','R','E','E','N','>'};
		byte[] packagePostfix =  new byte[]{'<','/','W','B','_','S','C','R','E','E','N','>'};
		try {
			prefixStr = new String(packagePrefix,"UTF-8");
			postfixStr = new String(packagePostfix, "UTF-8");
		} catch (UnsupportedEncodingException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	public byte[] handleNewComingData(byte[] data,int actualLength){
		String actualDataStr;
		try {
			actualDataStr = new String(data,0,actualLength,"UTF-8");
			strBuilder.append(actualDataStr);
			String currentDataStr = strBuilder.toString();
			if(currentDataStr.contains(prefixStr) && currentDataStr.contains(postfixStr)){
				int chunkStart = currentDataStr.indexOf(prefixStr);
				int chunkEnd = currentDataStr.indexOf(postfixStr);
				//Log.i("Chunk anchors", String.format("Start = %d - End = %d", chunkStart, chunkEnd));
				byte[]screenshotData =null;
				if(chunkStart<chunkEnd){
					Log.i("Package length", String.valueOf(chunkEnd + postfixStr.length() - chunkStart));
					 screenshotData = new byte[chunkEnd - (chunkStart + prefixStr.length())];
					 //Log.i("Screenshot bytelength", String.valueOf(screenshotData.length));
					 currentDataStr.getBytes(chunkStart + prefixStr.length(), chunkEnd, screenshotData, 0);
				}
				strBuilder.delete(0, chunkEnd + postfixStr.length()); 
				return screenshotData;
			}
		} catch (UnsupportedEncodingException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			
		}
		return null;
	}
	 
	
}
