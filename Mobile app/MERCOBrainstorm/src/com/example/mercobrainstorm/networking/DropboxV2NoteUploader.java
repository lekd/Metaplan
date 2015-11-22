package com.example.mercobrainstorm.networking;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

import com.dropbox.core.DbxException;
import com.dropbox.core.DbxRequestConfig;
import com.dropbox.core.v2.DbxClientV2;
import com.dropbox.core.v2.DbxFiles.Metadata;
import com.dropbox.core.v2.DbxFiles.UploadException;
import com.dropbox.core.v2.DbxFiles.WriteMode;
import com.dropbox.core.v2.DbxUsers.FullAccount;
import com.dropbox.core.v2.DbxUsers.GetCurrentAccountException;

import android.content.Context;
import android.os.AsyncTask;
import android.util.Log;
import android.widget.Toast;

public class DropboxV2NoteUploader extends AsyncTask<Object, Integer, Boolean>{
	
    DbxClientV2 dbxClient = null;
    String targetFolder = "/Notes/";
    Context mContext = null;
    public DropboxV2NoteUploader(Context ctx,DbxClientV2 client){
    	dbxClient = client;
    	mContext = ctx;
    }
	@Override
	protected Boolean doInBackground(Object... uploadParams) {
		// TODO Auto-generated method stub
		File toBeUploaded = (File)(uploadParams[0]);
        String targetFileName = (String)(uploadParams[1]);
        String fullPath = targetFolder + targetFileName;
        try {
			FileInputStream fis = new FileInputStream(toBeUploaded);
			dbxClient.files.uploadBuilder(fullPath).mode(WriteMode.overwrite).run(fis);
		} catch (FileNotFoundException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (UploadException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (DbxException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		return true;
	}
	@Override
    protected void onPostExecute(Boolean result) {
		if (result) {
            showToast("Image successfully uploaded");
        } else {
            showToast("Unable to upload file");
        }
	}
	private void showToast(String msg) {
        Toast error = Toast.makeText(mContext, msg, Toast.LENGTH_LONG);
        error.show();
    }
}
