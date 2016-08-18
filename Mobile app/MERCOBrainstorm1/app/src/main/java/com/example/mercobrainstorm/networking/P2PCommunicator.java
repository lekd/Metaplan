package com.example.mercobrainstorm.networking;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.Socket;
import java.net.UnknownHostException;

import android.util.Log;

public class P2PCommunicator extends Thread{
	String serverAddress;
	int dstPort = 2015;
	OutputStream outStream = null;
	InputStream inStream = null;
	byte[] inputBuffer = new byte[1024];
	Socket socket = null;
	IP2PCommunicationEventListener p2pEventListener = null;
	
	boolean stillworking;
	public void setP2PEventListener(IP2PCommunicationEventListener listener){
		p2pEventListener = listener;
	}
	public P2PCommunicator(String serverIP,int port){
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
			inStream = socket.getInputStream();
			if(outStream!=null){
				Log.i("WIFICONN", "Connected to " + serverAddress);
			}
			stillworking = true;
			while(stillworking){
				receiveData();
			}
		} catch (UnknownHostException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			if(p2pEventListener != null){
				p2pEventListener.UnknowHostEventListener();
			}
		}
	}
	
	public void Release(){
		stillworking = false;
		if(socket!=null){
			try {
				socket.close();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
	}
	public void receiveData(){
		int byteRead;
		try {
			byteRead = inStream.read(inputBuffer);
			if(byteRead > 0){
				if(p2pEventListener != null){
					p2pEventListener.DataReceivedEventListener(inputBuffer, byteRead);
					Log.i("ByteReceived", String.valueOf(byteRead));
				}
			}
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		
		
	}
	public void sendData(byte[] data){
		
		if(outStream==null || !stillworking){
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
	public interface IP2PCommunicationEventListener{
		void UnknowHostEventListener();
		void DataReceivedEventListener(byte[] data, int bytesRead);
	}
}
