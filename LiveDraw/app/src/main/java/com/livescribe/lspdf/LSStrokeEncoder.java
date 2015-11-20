package com.livescribe.lspdf;

public class LSStrokeEncoder
{
  static
  {
    System.loadLibrary("lspdf");
  }

  public static native int[] boundingBoxOfEncodedStroke(byte[] paramArrayOfByte);

  public static native LSStrokeEncoder$DecodedStroke decodeEncodedStroke(byte[] paramArrayOfByte, boolean paramBoolean);

  public static LSStrokeEncoder$DecodedStroke decodedStroke(long[] paramArrayOfLong, boolean paramBoolean, int[] paramArrayOfInt1, int[] paramArrayOfInt2)
  {
    return new LSStrokeEncoder$DecodedStroke(paramArrayOfLong, paramBoolean, paramArrayOfInt1, paramArrayOfInt2);
  }

  public static native byte[] encodeStroke(long[] paramArrayOfLong, int[] paramArrayOfInt1, int[] paramArrayOfInt2, long paramLong);

  public static native short numCoordsInEncodedStroke(byte[] paramArrayOfByte);

  public static native long pageAddressOfEncodedStroke(byte[] paramArrayOfByte);

  public static native long[] timeRangeOfEncodedStroke(byte[] paramArrayOfByte);
}

/* Location:           /home/ali/Documents/LivescribeHack/classes-dex2jar.jar
 * Qualified Name:     com.livescribe.lspdf.LSStrokeEncoder
 * JD-Core Version:    0.6.2
 */