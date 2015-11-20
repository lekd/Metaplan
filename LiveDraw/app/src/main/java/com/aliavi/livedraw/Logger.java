package com.aliavi.livedraw;

import android.util.Log;

/**
 * Created by ali on 11/19/15.
 */
public class Logger {
    private static final String TAG = "MyActivity";
    public static void log(String s)
    {
        if (s != null)
            Log.v(TAG, s);
    }
    public static void log(String format, Object... args)
    {
        log(String.format(format, args));
    }
}
