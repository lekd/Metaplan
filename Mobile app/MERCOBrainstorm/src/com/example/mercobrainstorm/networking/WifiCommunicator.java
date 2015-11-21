package com.example.mercobrainstorm.networking;

import java.io.IOException;
import java.io.OutputStream;
import java.net.Socket;
import java.net.UnknownHostException;

import android.util.Log;

public class WifiCommunicator extends Thread{
	String serverAddress;
	int dstPort = 2015;
	OutputStream outStream = null;
	Socket socket = null;
	
	public WifiCommunicator(String serverIP,int port){
		serverAddress = serverIP;
		if(port>0){
			dstPort = port;
		}
	}
	public void run() {
		// TODO Auto-generated method stub
		
		try {
			socket = new Socket(serverAddress,dstPort);
			socket.setTcpNoDelay(true);
			outStream = socket.getOutputStream();
			if(outStream!=null){
				Log.i("WIFICONN", "Connected to " + serverAddress);
			}
		} catch (UnknownHostException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	
	public void Release(){
		if(socket!=null){
			try {
				socket.close();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
	}
	public void sendData(byte[] data){
		
		if(outStream==null){
			return;
		}
		try {
			outStream.write(data);
			outStream.flush();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
	}
}
