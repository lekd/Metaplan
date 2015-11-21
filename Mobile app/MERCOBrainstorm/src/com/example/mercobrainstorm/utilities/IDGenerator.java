package com.example.mercobrainstorm.utilities;

import java.util.Random;

public class IDGenerator {
	static int saltValue;
	static boolean isInitialized = false;
	public static void Initialize(){
		Random rnd = new Random();
		saltValue = rnd.nextInt(Short.MAX_VALUE/2)+1;
		isInitialized = true;
	}
	static int hash(int a,int b){
		return ((a + b)*(a + b + 1)/2 + b);
	}
	public static int getHashedID(int inputID){
		if(!isInitialized){
			Initialize();
		}
		return hash(saltValue,inputID);
	}
}
