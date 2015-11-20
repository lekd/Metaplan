package com.livescribe.lspdf;

public class LSBinary$NotebookPageInfo
{
  public int cropHeight;
  public int cropWidth;
  public int cropX;
  public int cropY;
  public int mediaHeight;
  public int mediaWidth;
  public long pageAddress;
  public short pageNumber;

  public LSBinary$NotebookPageInfo(long paramLong, short paramShort, int paramInt1, int paramInt2, int paramInt3, int paramInt4, int paramInt5, int paramInt6)
  {
    this.pageAddress = paramLong;
    this.pageNumber = paramShort;
    this.mediaWidth = paramInt1;
    this.mediaHeight = paramInt2;
    this.cropX = paramInt3;
    this.cropY = paramInt4;
    this.cropWidth = paramInt5;
    this.cropHeight = paramInt6;
  }
}

/* Location:           /home/ali/Documents/LivescribeHack/classes-dex2jar.jar
 * Qualified Name:     com.livescribe.lspdf.LSBinary.NotebookPageInfo
 * JD-Core Version:    0.6.2
 */