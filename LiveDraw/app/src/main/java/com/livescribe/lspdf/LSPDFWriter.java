package com.livescribe.lspdf;

import android.content.Context;
import android.content.res.AssetManager;
import java.io.BufferedOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStream;

public class LSPDFWriter
{
  protected static boolean _pdfResourcesHaveBeenSet = false;

  static
  {
    System.loadLibrary("lspdf");
  }

  public LSPDFWriter(Context paramContext)
  {
    if (!_pdfResourcesHaveBeenSet)
      _pdfResourcesHaveBeenSet = _setPDFResourcesDirectory(paramContext);
  }

  protected static native void _setPDFResourcesDirectory(String paramString1, String paramString2, String paramString3);

  protected static boolean _setPDFResourcesDirectory(Context paramContext)
  {
    try
    {
      File localFile = paramContext.getDir("pdf", 0);
      String[] arrayOfString = paramContext.getAssets().list("pdf");
      int j = arrayOfString.length;
      int i = 0;
      Object localObject1;
      Object localObject2;
      localObject1 = arrayOfString[i];
      localObject2 = new File(localFile + "/" + (String)localObject1);
      while (true)
      {

        if (i < j)
        {
          localObject1 = arrayOfString[i];
          localObject2 = new File(localFile + "/" + (String)localObject1);
        }
        try
        {
          localObject1 = paramContext.getAssets().open("pdf/" + (String)localObject1);
          localObject2 = new FileOutputStream((File)localObject2);
          int k = ((InputStream)localObject1).available();
          byte[] arrayOfByte = new byte[k];
          ((InputStream)localObject1).read(arrayOfByte);
          BufferedOutputStream localBufferedOutputStream = new BufferedOutputStream((OutputStream)localObject2, arrayOfByte.length);
          localBufferedOutputStream.write(arrayOfByte, 0, k);
          localBufferedOutputStream.flush();
          localBufferedOutputStream.close();
          ((FileOutputStream)localObject2).close();
          ((InputStream)localObject1).close();
          i += 1;
          continue;
         // _setPDFResourcesDirectory(localFile.getAbsolutePath() + "/", "/system/fonts/Roboto-Regular.ttf", "/system/fonts/DroidSansFallback.ttf");
          //return true;
        }
        catch (Exception paramContext0)
        {
          return false;
        }
      }
    }
    catch (Exception paramContext1)
    {
    }
    return false;
  }

  public native boolean renderLivescribePDF(String paramString, long paramLong);

  public native boolean renderSnippetPDF(String paramString, long paramLong);
}

/* Location:           /home/ali/Documents/LivescribeHack/classes-dex2jar.jar
 * Qualified Name:     com.livescribe.lspdf.LSPDFWriter
 * JD-Core Version:    0.6.2
 */