package com.aliavi.livedraw;

import android.app.Activity;
import android.content.Context;
import android.content.SharedPreferences;

import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.channels.FileChannel;

import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

/**
 * Created by ali on 11/20/15.
 */

/* TODO: Fix hardcoded paths using Android methods */

public final class Main {

    private final int REFRESH_PERIOD = 1000;
    private final Context context;
    private int lastUploadedID;
    private int lastSavedLocalID;
    private SharedPreferences preferences;
    private Activity caller;
    private final String LASTSAVEDLOCALID = "lastSavedLocalID";
    private final String LASTUPLOADEDID = "lastUploadedID";
    public Main(Activity caller) {
        context = caller.getApplicationContext();
        preferences = caller.getSharedPreferences("config", Context.MODE_PRIVATE);
    }

    public void Start(){

        /* Only for debugging purposes */
        boolean onlyOnce = false;
        final boolean updateAll = false;


        if (onlyOnce)
        {
            Update(updateAll);
            return;
        }

        /* TODO: Choose update frequency carefully */
        final Main d = this;
        final Timer timer = new Timer();
        TimerTask t = new TimerTask() {
            @Override
            public void run() {
                d.Update(updateAll);
            }
        };

        timer.schedule(t, 0, REFRESH_PERIOD);

    }
    public void Update(boolean updateAll) {
        /* get last uploaded and stored strokes */
        lastUploadedID = preferences.getInt(LASTUPLOADEDID, 0);
        lastSavedLocalID = preferences.getInt(LASTSAVEDLOCALID, 0);
        // copydb();
        Logger.log("Last local id = %d", lastSavedLocalID);
        Logger.log("Last uploaded id = %d", lastUploadedID);
        //try {

        copydb();

        List<LivescribeRecord>  strokes = checkforupdates(updateAll ? 0 : lastSavedLocalID);

        /* Delete all existing files amd start anew */
        if (updateAll) {
            File[] filenames = context.getFilesDir().listFiles();
            for (File file: filenames)
                file.delete();
        }

        if (strokes != null && strokes.size()!= 0)
        {
            List<String> changedFiles = createFiles(strokes);
            Upload(changedFiles);
        }
    }

    private void copydb(){

        try {
            Runtime.getRuntime().exec(new String[]{"/system/bin/su","-c","cp /data/data/com.livescribe.companion/databases/livescribe.db* /data/data/com.aliavi.livedraw/"});
            Logger.log("copy succeeded!");

        } catch (IOException e) {
            e.printStackTrace();
            Logger.log("copy failed. Check stack trace");
        }
    }

    private List<String> createFiles(List<LivescribeRecord> strokes) {

        /* TODO: Store the files locally, and store a lastid locally as well and attach the changes from that id onwards and finally upload them
         */

        int last_local = -1;
        if (strokes == null || strokes.size() == 0)
            return null;
        ArrayList<String> changedFiles = new ArrayList<String>();
        try {
            // within the main method
            Logger.log("Records = %d", strokes.size());

            for (LivescribeRecord l : strokes) {
                String filename = Long.toString(l.pageaddress);
                FileOutputStream outputStream = context.openFileOutput(filename, Context.MODE_APPEND);
                Logger.log("saving");
                // Every stroke starts from a new line
                outputStream.write(l.toFloatString().getBytes());
                outputStream.write('\n');

                // client.files.uploadBuilder(PATH+l.pageaddress+".txt").run(in);
                // Last successful id

                last_local = l.id;
                if (!changedFiles.contains(filename))
                    changedFiles.add(filename);
            }

            Logger.log("Save completed!");
        }
        catch (Exception ex)
        {
            Logger.log(ex.getMessage());
            ex.printStackTrace();
        }
        finally {
            if (last_local > -1) {
                preferences.edit().putInt(LASTSAVEDLOCALID, last_local).commit();
                Logger.log("Last id locall = %d", last_local);
            }
            return changedFiles;
        }

    }

    private void Upload(List<String> filenames ) {
        if (filenames == null || filenames.size() == 0)
            return;
        try {
            Logger.log("Uploading %d files", filenames.size());
            DropboxHelper dbx = new DropboxHelper(context);
            Boolean result = dbx.execute(filenames).get();
            Logger.log("Uploading %s", (result == Boolean.TRUE ? "SUCCEED" : "FAILED"));

        } catch (Exception ex) {
            Logger.log("Error in createFiles");
            ex.printStackTrace();
        }
    }

    private List<LivescribeRecord> checkforupdates(int lastid) {
        //String filepath = "/data/data/com.aliavi.livedraw/livescribe.db";
        String filepath = "/data/data/com.aliavi.livedraw/livescribe.db";
        try {
            LivescribeReader lsReader = new LivescribeReader(filepath);
            List<LivescribeRecord> results = lsReader.getStrokes(lastid);
            Logger.log("%d", results.size());
            return results;
        } catch (Exception ex) {
            ex.printStackTrace();
            return null;
        }
    }
}
