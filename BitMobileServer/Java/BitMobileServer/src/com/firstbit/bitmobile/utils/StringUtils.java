package com.firstbit.bitmobile.utils;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;

public class StringUtils 
{
    public static Boolean isNullOrEmpty(String value)
    {
    	if(value==null)
    		return true;
    	return value.length()==0;
    }
    
    public static String getStreamContents(InputStream stream) throws IOException {

        StringBuilder content = new StringBuilder();

        BufferedReader reader = new BufferedReader(new InputStreamReader(stream, "UTF-8"));
        String lineSeparator = System.getProperty("line.separator");

        try {
            String line;
            while ((line = reader.readLine()) != null) {
                content.append(line + lineSeparator);
            }
            return content.toString();

        } finally {
            reader.close();
        }

    }    
}
