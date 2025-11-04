using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DuplicateDetect
{
    public class HashStorage
    {
        public void SaveHashes(Dictionary<string, FileHashInfo> fileHashes, string filePath)
        {
            var data = new HashStorageData
            {
                ScanDate = DateTime.Now,
                TotalFiles = fileHashes.Count,
                FileHashes = new Dictionary<string, FileHashInfo>()
            };

            foreach (var kvp in fileHashes)
            {
                data.FileHashes[kvp.Key] = kvp.Value;
            }

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(data, settings);
            File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
        }

        public HashStorageData LoadHashes(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Hash kayıt dosyası bulunamadı: {filePath}");
            }

            string json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var result = JsonConvert.DeserializeObject<HashStorageData>(json, settings);
            if (result == null)
                throw new Exception("Hash kayıt dosyası okunamadı.");
            
            return result;
        }
    }

    public class HashStorageData
    {
        public DateTime ScanDate { get; set; }
        public int TotalFiles { get; set; }
        public Dictionary<string, FileHashInfo> FileHashes { get; set; }

        public HashStorageData()
        {
            FileHashes = new Dictionary<string, FileHashInfo>();
        }
    }
}

