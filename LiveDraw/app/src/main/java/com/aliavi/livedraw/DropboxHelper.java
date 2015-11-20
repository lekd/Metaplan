package com.aliavi.livedraw;

/**
 * Created by ali on 11/19/15.
 */
import android.content.Context;
import android.os.AsyncTask;

import com.dropbox.core.DbxRequestConfig;
import com.dropbox.core.v2.*;
import com.dropbox.core.*;


import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.io.*;
import java.util.ArrayList;
import java.util.Map;

public class DropboxHelper extends AsyncTask<List<String>, Integer, Boolean> {
    private final String ACCESS_TOKEN = "s7WRjU1-5tAAAAAAAAAAFzJl0lSiomHKqtk2okKxiKCZ3WIKIFgZRfvFNVusv2oT";
    private final String PATH = "/FromLivescribe/";
    private final Context ctx;
    @Override
    protected Boolean doInBackground(List<String>... params) {

        List<String> filenames = params[0];

        DbxRequestConfig config = new DbxRequestConfig(
                "LiveDraw", Locale.getDefault().toString());

        DbxClientV2 client = new DbxClientV2(config, ACCESS_TOKEN);
        try {
            DbxUsers.FullAccount account = client.users.getCurrentAccount();
            Logger.log("Dbxname = %s", account.name);

            // within the main method
            Logger.log("Files = %d", filenames.size());

            for (String filename : filenames) {
                String fullname = PATH+filename+".txt";
                Logger.log("uploading to %s", fullname);
                client.files.uploadBuilder(fullname).mode(DbxFiles.WriteMode.overwrite).run(ctx.openFileInput(filename));
            }

            Logger.log("Upload completed!");
            return Boolean.TRUE;
        }
        catch (Exception ex)
        {
            Logger.log(ex.getMessage());
            ex.printStackTrace();
        }
        return Boolean.FALSE;
    }
    DropboxHelper(Context ctx)
    {
        this.ctx = ctx;
    }
}
