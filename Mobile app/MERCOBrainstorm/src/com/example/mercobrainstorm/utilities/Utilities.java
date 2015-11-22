package com.example.mercobrainstorm.utilities;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.ByteBuffer;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Bitmap.CompressFormat;

public class Utilities {
	static public byte[] int2ByteArray(int i)
	{
	  byte[] result = new byte[4];

	  result[0] = (byte) (i >> 24);
	  result[1] = (byte) (i >> 16);
	  result[2] = (byte) (i >> 8);
	  result[3] = (byte) (i /*>> 0*/);

	  return result;
	}
	public static byte [] long2ByteArray (long value)
	{
	    return ByteBuffer.allocate(8).putLong(value).array();
	}

	public static byte [] float2ByteArray (float value)
	{  
	     return ByteBuffer.allocate(4).putFloat(value).array();
	}
	public static byte[] Bitmap2Bytes(Bitmap bmp){
		ByteArrayOutputStream stream = new ByteArrayOutputStream();
		bmp.compress(CompressFormat.PNG, 0, stream);
		return stream.toByteArray();
	}
	public static File Bitmap2File(Context ctx,Bitmap bmp, String fileName){
		File f = new File(ctx.getCacheDir(),fileName);
		try {
			f.createNewFile();
			ByteArrayOutputStream bos = new ByteArrayOutputStream();
			bmp.compress(CompressFormat.PNG, 0, bos);
			byte[] bitmapData = bos.toByteArray();
			
			FileOutputStream fos = new FileOutputStream(f);
			fos.write(bitmapData);
			fos.flush();
			fos.close();
			return f;
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		return null;
		
	}
}
