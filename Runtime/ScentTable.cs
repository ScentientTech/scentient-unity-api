using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using NReco.Csv;
using UnityEngine;

public class ScentTable : AbstractCsvData
{
    public readonly string name = "scent_table";


    public bool ScentTableLoadedSuccessfully {
        private set;
        get;
    }

    public async void Load()
    {
        const string url = "https://docs.google.com/spreadsheets/u/0/d/147rxjsbxxywS3lDJS17ZBHqmWqVh5phx6toxvDXsCz4/export?format=csv&id=147rxjsbxxywS3lDJS17ZBHqmWqVh5phx6toxvDXsCz4&gid=0";
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
    }

    public int GetScentIdByName(string scentName)
    {
        int row = FindRow(1,scentName.ToLower());
        if(row == -1){
            Debug.LogWarning($"Scent {scentName} not found in table");
            return 0;
        }
        return GetInt(0,row);
    
    }

    private void LoadCache()
    {
        var file = getFilepath();
        if(File.Exists(file)){
            var dataStream = new StreamReader(file);
            var reader = new CsvReader(dataStream,",");
            
            Parse(reader);
            ScentTableLoadedSuccessfully = true;
            
        } 
        else {
            Debug.LogError($"Unable to load CSV file {file}");
        }
    }

    private void SaveCache(Stream stream)
    {
        using( var writeStream = File.OpenWrite(getFilepath()) ){
            stream.CopyTo( writeStream );
        }
    }

    string getFilepath(){
        string dir = Application.persistentDataPath;
        string fallbackDir = dir;
        string path = string.Empty;
        path = Path.Combine( dir, string.Format("{0}.csv",this.name.ToLower().Replace(' ','-')));
        
        return path;        
    }
    
}
