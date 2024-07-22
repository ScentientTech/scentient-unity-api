using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using NReco.Csv;
using UnityEngine;
namespace Scentient
{
    public class ScentTable : AbstractCsvData
    {
        public event Action loadSucessfulEvent;
        public event Action loadFailedEvent;
        public readonly string name = "scent_table";

        

        public bool Loaded
        {
            private set;
            get;
        }
        public bool ScentTableLoadedSuccessfully
        {
            private set;
            get;
        }

        public async void Load()
        {
            
            
            //UnityEngine.Android.Permission.;
            const string url = "https://api.scentient.tech/scent-table_en.csv";
            HttpClient req = new HttpClient();
            try
            {

                var response = await req.GetAsync(url);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Debug.LogWarning($"Unable to load scent csv data, from {url} \nserver responded with:{((int)response.StatusCode)} {response.StatusCode}");
                    LoadCache();
                    return;
                }

                var contentStream = await response.Content.ReadAsStreamAsync();
                using (StreamReader textReader = new StreamReader(contentStream))
                {
                    var reader = new CsvReader(textReader, ",");
                    Parse(reader);
                    textReader.DiscardBufferedData();
                    textReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    SaveCache(contentStream);
                }
                contentStream.Close();
                ScentTableLoadedSuccessfully = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Unable to load csv data, from {url} \nerror: {e.ToString()}");
                LoadCache();
                return;
            }
            Loaded = true;
            loadSucessfulEvent?.Invoke();
        }

        public bool GetScentIdByName(string scentName, out int id)
        {
            id = 0;
            int row = FindRow(1, scentName.ToLower());
            if (row == -1)
            {
                Debug.LogWarning($"Scent {scentName} not found in table");
                return false;
            }
            if (!TryGetInt(0, row, out int result))
            {
                Debug.LogWarning($"Scent {scentName} has bad id");
                return false;
            }
            id = result;
            return true;
        }

        public string GetScentNameById(short scentId)
        {
            int row = FindRow(0, scentId);
            if (row == -1)
            {
                Debug.LogWarning($"Scent {scentId} not found in table");
                return $"{scentId} unknown";
            }
            if (!TryGetString(1, row, out string result))
            {
                Debug.LogWarning($"Scent {scentId} has bad id");
                return $"{scentId} unknown";
            }
            return result;
        }

        private bool LoadCache()
        {
            var file = GetFilepath();

            //If file doesn't exist in filesystem attempt to copy scent table embedded in app            
            if (!File.Exists(file))
            {
               var embeddedScentTable = Resources.Load<TextAsset>("scent-table_en");
               if(embeddedScentTable==null){
                    Debug.LogWarning("unable to load embedded scent table");
               }           
               else {
                   File.WriteAllText(file,embeddedScentTable.text);
               }    
            }
            if (File.Exists(file))
            {
                var dataStream = new StreamReader(file);
                var reader = new CsvReader(dataStream, ",");
                Parse(reader);
                ScentTableLoadedSuccessfully = true;
                loadSucessfulEvent?.Invoke();
                return true;
            }
            else
            {
                Debug.LogError($"Unable to load CSV file {file}");
                loadFailedEvent?.Invoke();
                return false;
            }
        }

        private void SaveCache(Stream stream)
        {
            using (var writeStream = File.OpenWrite(GetFilepath()))
            {
                stream.CopyTo(writeStream);
            }
        }

        string GetFilepath()
        {
            string dir = Application.persistentDataPath;
            string fallbackDir = dir;
            string path = string.Empty;
            path = Path.Combine(dir, string.Format("{0}.csv", this.name.ToLower().Replace(' ', '-')));

            return path;
        }

    }
}