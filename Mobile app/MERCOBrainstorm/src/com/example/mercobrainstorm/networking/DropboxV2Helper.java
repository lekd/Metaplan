package com.example.mercobrainstorm.networking;

import java.util.Locale;

import com.dropbox.core.DbxRequestConfig;
import com.dropbox.core.v2.DbxClientV2;

public class DropboxV2Helper {
	private static final String ACCESS_TOKEN = "s7WRjU1-5tAAAAAAAAAA8c2K-AZCSrIGg2vC-eWwthEKEwY2S4fIQnpIYz4LyQNI";
    static DbxClientV2 client = null;
    static public void Init(){
    	DbxRequestConfig config = new DbxRequestConfig(
                "MetaplanMobile", Locale.getDefault().toString());

        client = new DbxClientV2(config, ACCESS_TOKEN);
    }
    public static DbxClientV2 getDbxClient(){
    	return client;
    }
}
