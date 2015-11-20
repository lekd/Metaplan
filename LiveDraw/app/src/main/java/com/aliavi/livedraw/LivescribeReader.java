package com.aliavi.livedraw;

import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by ali on 11/19/15.
 */
public class LivescribeReader {
    private final SQLiteDatabase db;
    /* Column index in 'event_paper_stroke' table */
    private final int ENCODED_COORDINATES = 8;
    private final int PAGE_ADDRESS = 1;
    private final int ID = 0;

    LivescribeReader(String filePath) throws Exception
    {
        db = SQLiteDatabase.openDatabase(filePath, null, SQLiteDatabase.ENABLE_WRITE_AHEAD_LOGGING);
        //db.execSQL("PRAGMA wal_checkpoint;");
    }

    public List<LivescribeRecord> getStrokes() throws Exception
    {
        return getStrokes(0);
    }

    /**
     * Returns list of all strokes with ids larger than last_id, excluding last_id itself
     * @param last_id
     * @return A list of LivescribeRecords
     * @throws Exception
     */
    public List<LivescribeRecord> getStrokes(int last_id) throws Exception
    {

        Cursor resultSet = db.rawQuery("Select * from event_paper_stroke where (_id > " + Integer.toString(last_id) + ");", null);
        List<LivescribeRecord> result = new ArrayList<>();

        while (resultSet.moveToNext()) {
            try {

                long pageaddress = resultSet.getLong(PAGE_ADDRESS);
                int id = resultSet.getInt(ID);
                byte[] encodedCoordinates = resultSet.getBlob(ENCODED_COORDINATES);

                result.add(new LivescribeRecord(id, pageaddress, encodedCoordinates));
            } catch (Exception ex)
            {
                ex.printStackTrace();
            }
        }
        return result;
    }


}
