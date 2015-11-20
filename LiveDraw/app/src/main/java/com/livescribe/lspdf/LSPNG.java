package com.livescribe.lspdf;

public class LSPNG
{
  static
  {
    System.loadLibrary("lspdf");
  }

  public static native int resamplePNG(String paramString1, String paramString2, short paramShort1, short paramShort2);
}

/* Location:           /home/ali/Documents/LivescribeHack/classes-dex2jar.jar
 * Qualified Name:     com.livescribe.lspdf.LSPNG
 * JD-Core Version:    0.6.2
 */