package com.livescribe.lspdf;

import android.graphics.Point;
import android.graphics.Rect;

public class LSGeometry
{
  static
  {
    System.loadLibrary("lspdf");
  }

  public static native float[] generateCurveForEncodedPoints(byte[] paramArrayOfByte, boolean paramBoolean);

  protected static native float[] generateCurveForPoints(int[] paramArrayOfInt, boolean paramBoolean);

  public static float[] generateCurveForPoints(Point[] paramArrayOfPoint, boolean paramBoolean)
  {
    int i = 0;
    if ((paramArrayOfPoint == null) || (paramArrayOfPoint.length == 0))
      return null;
    int[] arrayOfInt = new int[paramArrayOfPoint.length * 2];
    int k = paramArrayOfPoint.length;
    int j = 0;
    while (i < k)
    {
      Point localPoint = paramArrayOfPoint[i];
      int m = j + 1;
      arrayOfInt[j] = localPoint.x;
      j = m + 1;
      arrayOfInt[m] = localPoint.y;
      i += 1;
    }
    return generateCurveForPoints(arrayOfInt, paramBoolean);
  }

  public static float[] transform(Rect paramRect1, Rect paramRect2)
  {
    if ((paramRect1 == null) || (paramRect2 == null))
      return null;
    int i = paramRect1.left;
    int j = paramRect1.top;
    int k = paramRect1.width();
    int m = paramRect1.height();
    int n = paramRect2.left;
    int i1 = paramRect2.top;
    int i2 = paramRect2.width();
    int i3 = paramRect2.height();
    return transform(new int[] { i, j, k, m }, new int[] { n, i1, i2, i3 });
  }

  public static native float[] transform(int[] paramArrayOfInt1, int[] paramArrayOfInt2);
}

/* Location:           /home/ali/Documents/LivescribeHack/classes-dex2jar.jar
 * Qualified Name:     com.livescribe.lspdf.LSGeometry
 * JD-Core Version:    0.6.2
 */