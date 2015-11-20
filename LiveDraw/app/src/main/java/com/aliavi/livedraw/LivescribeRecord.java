package com.aliavi.livedraw;

import android.graphics.PointF;

import com.livescribe.lspdf.LSGeometry;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by ali on 11/19/15.
 */
public class LivescribeRecord {

    LivescribeRecord(int id, long pageaddress, byte[] encoded_coordinates)
    {

        this.pageaddress = pageaddress;
        this.id = id;
        /* Get encoded data from db and convert it using the JNI method LSGeometry.generateCurveForEncodedPoints
         * TODO: Test what's the second boolean argument in LSGeometry.generateCurveForEncodedPoints */
        try {
            float[] floats = LSGeometry.generateCurveForEncodedPoints(encoded_coordinates, false);

        /* Convert float array indicating the curve to PointF objects */

            for (int i = 0; i < floats.length / 2; i++)
                points.add(new PointF(floats[2 * i], floats[2 * i + 1]));
        }
        catch (Exception ex)
        {
            Logger.log(ex.toString());
        }
    }

    public final int id;
    public final long pageaddress;
    public final List<PointF> points = new ArrayList<PointF>();

    public String toFloatString()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < points.size(); i++) {
            sb.append(points.get(i).x);
            sb.append(' ');
            sb.append(points.get(i).y);
            sb.append(' ');
        }

        return sb.toString();
    }
}
