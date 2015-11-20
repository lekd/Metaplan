package com.livescribe.lspdf;

public class LSStrokeEncoder$DecodedStroke
{
  public boolean strokeTimesAbsolute;
  public long[] times;
  public int[] x;
  public int[] y;

  public LSStrokeEncoder$DecodedStroke(long[] paramArrayOfLong, boolean paramBoolean, int[] paramArrayOfInt1, int[] paramArrayOfInt2)
  {
    this.times = paramArrayOfLong;
    this.strokeTimesAbsolute = paramBoolean;
    this.x = paramArrayOfInt1;
    this.y = paramArrayOfInt2;
  }
}

/* Location:           /home/ali/Documents/LivescribeHack/classes-dex2jar.jar
 * Qualified Name:     com.livescribe.lspdf.LSStrokeEncoder.DecodedStroke
 * JD-Core Version:    0.6.2
 */