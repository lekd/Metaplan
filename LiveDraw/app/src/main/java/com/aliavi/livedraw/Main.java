package com.aliavi.livedraw;

import android.app.Activity;
import android.content.Context;
import android.content.SharedPreferences;

import java.io.DataOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

/**
 * Created by ali on 11/20/15.
 */
public final class Main {
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

        /* get last uploaded and stored strokes */
        lastUploadedID = preferences.getInt(LASTUPLOADEDID, 0);
        lastSavedLocalID = preferences.getInt(LASTSAVEDLOCALID, 0);
    }

    public void Start(){

        /* Only for debugging purposes */
        boolean onlyOnce = false;
        final boolean updateAll = true;


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

        timer.schedule(t, 0, 5000);

    }
    public void Update(boolean updateAll) {
        // copydb();
        Logger.log("Last local id = %d", lastSavedLocalID);
        Logger.log("Last uploaded id = %d", lastUploadedID);

        List<LivescribeRecord>  strokes = checkforupdates(updateAll ? 0 : lastSavedLocalID);

        /* Delete all existing files amd start anew */
        if (updateAll) {
            File[] filenames = context.getFilesDir().listFiles();
            for (File file: filenames)
                file.delete();
        }

        if (strokes != null && strokes.size()!= 0)
            createFiles(strokes);
    }

    private void copydb() {
        Process p;
        try {
            // Preform su to get root privledges
            p = Runtime.getRuntime().exec("su");

            // Attempt to write a file to a root-only
            DataOutputStream os = new DataOutputStream(p.getOutputStream());
            os.writeBytes("cp /data/data/com.livescribe.companion/databases/livescribe.db /data/data/com.aliavi.livedraw/\n");

            // Close the terminal
            os.writeBytes("exit\n");
            os.flush();
        } catch (IOException e) {
            e.printStackTrace();
            Logger.log("copy failed. Check stack trace");
        }
    }

    private void createFiles(List<LivescribeRecord> strokes) {

        /* TODO: Store the files locally, and store a lastid locally as well and attach the changes from that id onwards and finally upload them
         */

        int last_local = -1;
        if (strokes == null || strokes.size() == 0)
            return;

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
        }

        try {
            String[] filenames = context.getFilesDir().list();
            Logger.log("%d files found", filenames.length);
            DropboxHelper dbx = new DropboxHelper(context);
            Boolean result = dbx.execute(filenames).get();

            Logger.log("Uploaded up to %s", (result == Boolean.TRUE ? "SUCCESS" : "FAILURE"));


        } catch (Exception ex) {
            Logger.log("Error in createFiles");
            ex.printStackTrace();
        }

    }

    private List<LivescribeRecord> checkforupdates(int lastid) {
        String filepath = "/data/data/com.aliavi.livedraw/livescribe.db";
        //String filePath = "/data/data/com.livescribe.companion/databases/livescribe.db";
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
