package com.livescribe.lspdf;

public class LSBinary
{
  static
  {
    System.loadLibrary("lspdf");
  }

  public static native void addBackgroundBytes(long paramLong, String paramString, byte[] paramArrayOfByte);

  public static native void addBackgroundImageKey(long paramLong1, long paramLong2, String paramString);

  public static native void addBackgroundKeysAndImagesFromAFD(long paramLong1, long paramLong2, long[] paramArrayOfLong);

  public static native void addBackgroundPath(long paramLong, String paramString1, String paramString2);

  public static native long addNotebook(long paramLong1, long paramLong2, long paramLong3, int paramInt);

  public static native long addNotebookPage(long paramLong1, long paramLong2, long paramLong3);

  public static native void addNotebookPageStroke(long paramLong, byte[] paramArrayOfByte);

  public static native void addPNGImageBytesSnippet(long paramLong, byte[] paramArrayOfByte, String[] paramArrayOfString);

  public static native void addPNGImagePathSnippet(long paramLong, String paramString, String[] paramArrayOfString);

  public static native long addPencast(long paramLong1, String paramString, long paramLong2, long paramLong3, long paramLong4, long paramLong5);

  public static native void addPencastAudioBytes(long paramLong1, long paramLong2, long paramLong3, byte[] paramArrayOfByte);

  public static native void addPencastAudioPath(long paramLong1, long paramLong2, long paramLong3, String paramString);

  public static native void addPencastStrokeInfo(long paramLong1, long paramLong2, long paramLong3, long paramLong4, long paramLong5);

  public static native long addStrokeSnippet(long paramLong, int paramInt1, int paramInt2, int paramInt3, int paramInt4, String[] paramArrayOfString);

  public static native void addStrokeToSnippet(long paramLong, byte[] paramArrayOfByte);

  public static native void addTextSnippet(long paramLong, String[] paramArrayOfString);

  public static native String afdNotebookName(long paramLong);

  public static native int afdPageNumberForPageAddress(long paramLong1, long paramLong2);

  public static native String getAppName(long paramLong);

  public static native byte[] getBackgroundBytes(long paramLong, String paramString);

  public static native String getBackgroundImageKey(long paramLong1, long paramLong2);

  public static native String getBackgroundPath(long paramLong, String paramString);

  public static native byte[] getBytesFromWriteContext(long paramLong);

  public static native long getNotebook(long paramLong, int paramInt);

  public static native LSBinary$NotebookInfo getNotebookInfo(long paramLong);

  public static native long getNotebookPage(long paramLong, int paramInt);

  public static native LSBinary$NotebookPageInfo getNotebookPageInfo(long paramLong);

  public static native byte[] getNotebookPageStroke(long paramLong, int paramInt);

  public static native LSBinary$PenInfo getPen(long paramLong);

  public static native long getPencast(long paramLong, int paramInt);

  public static native LSBinary$PencastAudio getPencastAudioBytes(long paramLong, int paramInt);

  public static native LSBinary$PencastAudio getPencastAudioPath(long paramLong, int paramInt);

  public static native LSBinary$PencastInfo getPencastInfo(long paramLong);

  public static native LSBinary$PencastStroke getPencastStrokeInfo(long paramLong, int paramInt);

  public static native String json(long paramLong);

  public static native long newAFDContext(String paramString);

  public static LSBinary$NotebookInfo newNotebookInfo(long paramLong, short paramShort1, short paramShort2)
  {
    return new LSBinary$NotebookInfo(paramLong, paramShort1, paramShort2);
  }

  public static LSBinary$NotebookPageInfo newNotebookPageInfo(long paramLong, short paramShort, int paramInt1, int paramInt2, int paramInt3, int paramInt4, int paramInt5, int paramInt6)
  {
    return new LSBinary$NotebookPageInfo(paramLong, paramShort, paramInt1, paramInt2, paramInt3, paramInt4, paramInt5, paramInt6);
  }

  public static LSBinary$PenInfo newPenInfo(long paramLong, short paramShort)
  {
    return new LSBinary$PenInfo(paramLong, paramShort);
  }

  public static LSBinary$PencastAudio newPencastAudioBytes(long paramLong1, long paramLong2, byte[] paramArrayOfByte)
  {
    return new LSBinary$PencastAudio(paramLong1, paramLong2, paramArrayOfByte);
  }

  public static LSBinary$PencastAudio newPencastAudioPath(long paramLong1, long paramLong2, String paramString)
  {
    return new LSBinary$PencastAudio(paramLong1, paramLong2, paramString);
  }

  public static LSBinary$PencastInfo newPencastInfo(String paramString, long paramLong1, long paramLong2, long paramLong3, long paramLong4)
  {
    return new LSBinary$PencastInfo(paramString, paramLong1, paramLong2, paramLong3, paramLong4);
  }

  public static LSBinary$PencastStroke newPencastStroke(long paramLong1, long paramLong2, long paramLong3, long paramLong4)
  {
    return new LSBinary$PencastStroke(paramLong1, paramLong2, paramLong3, paramLong4);
  }

  public static native long newReadContextFromPDF(String paramString);

  public static native long newReadContextFromWriteContext(long paramLong);

  public static native long newWriteContext();

  public static native int numAudioFiles(long paramLong);

  public static native int numNotebookPages(long paramLong);

  public static native int numNotebooks(long paramLong);

  public static native int numPencastStrokeInfo(long paramLong);

  public static native int numPencasts(long paramLong);

  public static native int numStrokesForPage(long paramLong);

  public static native void releaseAFDContext(long paramLong);

  public static native void releaseBinaryContext(long paramLong);

  public static native void setAppName(long paramLong, String paramString);

  public static native void setPen(long paramLong1, long paramLong2, short paramShort);
}

/* Location:           /home/ali/Documents/LivescribeHack/classes-dex2jar.jar
 * Qualified Name:     com.livescribe.lspdf.LSBinary
 * JD-Core Version:    0.6.2
 */