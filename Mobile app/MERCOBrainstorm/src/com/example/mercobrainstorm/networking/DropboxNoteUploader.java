package com.example.mercobrainstorm.networking;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;

import com.dropbox.client2.DropboxAPI;
import com.dropbox.client2.DropboxAPI.UploadRequest;
import com.dropbox.client2.ProgressListener;
import com.dropbox.client2.exception.DropboxException;

import android.content.Context;
import android.os.AsyncTask;
import android.util.Log;
import android.widget.Toast;

public class DropboxNoteUploader extends AsyncTask<Void, Long, Boolean>{
	private static String TAG =  "UPLOADER";
	private DropboxAPI<?> mApi;
    private String mPath;
    private File mFile;

    private long mFileLen;
    private UploadRequest mRequest;
    private Context mContext;
    
    private String mErrorMsg;
    
    public DropboxNoteUploader(Context context, DropboxAPI<?> api, String dropboxPath,
            File file){
    	mContext = context.getApplicationContext();

        mFileLen = file.length();
        mApi = api;
        mPath = dropboxPath;
        mFile = file;
    }
	@Override
	protected Boolean doInBackground(Void... arg0) {
		// TODO Auto-generated method stub
		try {
			FileInputStream fis = new FileInputStream(mFile);
			String path = mPath + mFile.getName();
			mRequest = mApi.putFileOverwriteRequest(path, fis, mFile.length(),
					new ProgressListener(){
                @Override
                public long progressInterval() {
                    // Update the progress bar every half-second or so
                    return 500;
                }

                @Override
                public void onProgress(long bytes, long total) {
                    
                }
            });
			if (mRequest != null) {
				mRequest.upload();
	            return true;
	        }
		} catch (FileNotFoundException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			Log.i(TAG, e.getMessage());
		} catch (DropboxException e) {
			// TODO Auto-generated catch block
			Log.i(TAG, e.getMessage());
			mErrorMsg = e.getMessage();
			e.printStackTrace();
		}
        
		return false;
	}
	@Override
    protected void onPostExecute(Boolean result) {
		if (result) {
            showToast("Image successfully uploaded");
        } else {
            showToast(mErrorMsg);
        }
	}
	private void showToast(String msg) {
        Toast error = Toast.makeText(mContext, msg, Toast.LENGTH_LONG);
        error.show();
    }
}
