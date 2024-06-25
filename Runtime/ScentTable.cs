using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using NReco.Csv;
using UnityEngine;

public class ScentTable : AbstractCsvData
{
    public event Action loadSucessfulEvent;
    public event Action loadFailedEvent;
    public readonly string name = "scent_table";

    public bool Loaded {
        private set;
        get;
    }
    public bool ScentTableLoadedSuccessfully {
        private set;
        get;
    }

    public async void Load()
    {
        //UnityEngine.Android.Permission.;
        const string url = "https://api.scentient.tech/scent-table_en.csv";
        HttpClient req = new HttpClient();
        try{ 

            var response = await req.GetAsync(url);
        
            if(response.StatusCode!=System.Net.HttpStatusCode.OK){
                Debug.LogWarning($"Unable to load scent csv data, from {url} \nserver responded with:{((int)response.StatusCode)} {response.StatusCode}");
                LoadCache();
                return;
            }

            var contentStream = await response.Content.ReadAsStreamAsync();
            using (StreamReader textReader = new StreamReader(contentStream)){
                var reader = new CsvReader(textReader,",");
                Parse(reader);
                textReader.DiscardBufferedData();
                textReader.BaseStream.Seek(0, SeekOrigin.Begin);
                SaveCache(contentStream);
            }
            contentStream.Close();
            ScentTableLoadedSuccessfully = true;
        }
        catch(System.Exception e){
            Debug.LogWarning($"Unable to load csv data, from {url} \nerror: {e.ToString()}");
            LoadCache();
            return;
        }
        Loaded = true;
        loadSucessfulEvent?.Invoke();
    }

    public int GetScentIdByName(string scentName)
    {
        int row = FindRow(1,scentName.ToLower());
        if(row == -1){
            Debug.LogWarning($"Scent {scentName} not found in table");
            return 0;
        }
        if( !TryGetInt(0,row,out int result) ){
            Debug.LogWarning($"Scent {scentName} has bad id");
            return -1;
        }
        return result;
    }

    public string GetScentNameById(short scentId)
    {
        int row = FindRow(0,scentId);
        if(row == -1){
            Debug.LogWarning($"Scent {scentId} not found in table");
            return $"{scentId} unknown";
        }
        if( !TryGetString(1,row,out string result) ){
            Debug.LogWarning($"Scent {scentId} has bad id");
            return $"{scentId} unknown";
        }
        return result;
    }

    private void LoadCache()
    {
        var file = GetFilepath();
        if(File.Exists(file)){
            var dataStream = new StreamReader(file);
            var reader = new CsvReader(dataStream,",");
            
            Parse(reader);
            ScentTableLoadedSuccessfully = true;
            loadSucessfulEvent?.Invoke();   
        } 
        else {
            Debug.LogError($"Unable to load CSV file {file}");
            loadFailedEvent?.Invoke();
        }
    }

    private void SaveCache(Stream stream)
    {
        using( var writeStream = File.OpenWrite(GetFilepath()) ){
            stream.CopyTo( writeStream );
        }
    }

    string GetFilepath()
    {
        string dir = Application.persistentDataPath;
        string fallbackDir = dir;
        string path = string.Empty;
        path = Path.Combine( dir, string.Format("{0}.csv",this.name.ToLower().Replace(' ','-')));
        
        return path;
    }
    
}
