package com.livescribe.lspdf;

public class LSBinary$PencastAudio
{
  public byte[] bytes;
  public long end;
  public String path;
  public long start;

  public LSBinary$PencastAudio(long paramLong1, long paramLong2, String paramString)
  {
    this.start = paramLong1;
    this.end = paramLong2;
    this.path = paramString;
  }

  public LSBinary$PencastAudio(long paramLong1, long paramLong2, byte[] paramArrayOfByte)
  {
    this.start = paramLong1;
    this.end = paramLong2;
    this.bytes = paramArrayOfByte;
  }
}

/* Location:           /home/ali/Documents/LivescribeHack/classes-dex2jar.jar
 * Qualified Name:     com.livescribe.lspdf.LSBinary.PencastAudio
 * JD-Core Version:    0.6.2
 */