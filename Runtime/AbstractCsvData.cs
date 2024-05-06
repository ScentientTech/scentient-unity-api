/* 
AbstractCsvData.cs Copyright 2024 Paul Hayes

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the “Software”), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections.Generic;
using NReco.Csv;
using UnityEngine;

public abstract class AbstractCsvData 
{
    protected string[][] values;

    protected void Parse(CsvReader reader)
    {
        List<string[]> lines = new List<string[]>(); 
        while(reader.Read()){
            string[] row = new string[reader.FieldsCount];
            for (int i=0; i<reader.FieldsCount; i++) {
                row[i] = reader[i];
            }
            if(!row[0].StartsWith("#")){
                lines.Add(row);
            }
        }
        values = lines.ToArray();
    }
    public int GetInt(int col, int row)
    {
        return int.Parse(values[row][col]); 
    }

    public float GetFloat(int col, int row)
    {
        float result=0;
        if(!float.TryParse(values[row][col],out result)){
            Debug.LogWarning($"Unable to parse line {row}, column {col} as float");
        }
        return result; 

    }

    public string GetString(int col, int row)
    {
        if(values.Length<=row){
            return string.Empty;
        }
        if(values[row].Length<=col){
            return string.Empty;
        }

        return values[row][col].Trim('"');
    }

    public int FindRow(int searchCol,string value)
    {
        for( int i=0;i<values.Length;i++){
            var cols = values[i];
            if(cols[searchCol]==value){
                return i;
            }
        }
        return -1;
    }

    public int FindRows(int searchCol,string value, int[] matches)
    {
        int count = 0;
        for( int i=0;i<values.Length;i++){
            var cols = values[i];
            if(cols[searchCol]==value){
                matches[count++] = i;
            }
        }
        return count;
    }

    public int RowCount()
    {
        return values.Length;
    }

}