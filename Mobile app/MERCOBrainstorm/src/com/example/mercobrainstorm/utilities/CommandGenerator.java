package com.example.mercobrainstorm.utilities;

public class CommandGenerator {
	static byte[] ADD_Prefix = new byte[] { (byte)'<', (byte)'A', (byte)'D', (byte)'D', (byte)'>' };
    static byte[] ADD_Postfix = new byte[] { (byte)'<', (byte)'/', (byte)'A', (byte)'D', (byte)'D', (byte)'>' };
    public static byte[] GenerateADDCommand(byte[] bytesToBeAdded){
    	byte[] commandBytes = new byte[ADD_Prefix.length +  bytesToBeAdded.length + ADD_Postfix.length];
    	int index = 0;
    	System.arraycopy(ADD_Prefix, 0, commandBytes, index, ADD_Prefix.length);
    	index += ADD_Prefix.length;
    	
    	System.arraycopy(bytesToBeAdded, 0, commandBytes, index, bytesToBeAdded.length);
    	index += bytesToBeAdded.length;
    	
    	System.arraycopy(ADD_Postfix, 0, commandBytes, index, ADD_Postfix.length);
    	return commandBytes;
    }
}
