using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Globalization.DateTimeFormatting;
using Windows.Storage;
using Newtonsoft.Json;

namespace Galcorp.Auth.UWP
{
    public class TokenStore : IStore
    {
        private readonly string fileName = "galcorp.auth.cache.json";

        public async Task<T> Read<T>(string key) where T : class
        {
            var temporaryFolder = ApplicationData.Current.TemporaryFolder;

            var storage =  await GetStore(temporaryFolder);
            
            if (storage != null && storage.ContainsKey(key))
                return (T)storage[key];

            return null;
        }

        public async Task Store(string key, object value)
        {
            var temporaryFolder = ApplicationData.Current.TemporaryFolder;

            var storage = await GetStore(temporaryFolder);
            
            if (storage == null)
                storage = new Dictionary<string, object>();

            if (!storage.ContainsKey(key))
                storage.Add(key, value);
            else
                storage[key] = value;

            var text = JsonConvert.SerializeObject(storage, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            var sampleFile = await temporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, text);
        }

        private async Task<Dictionary<string, object>> GetStore(StorageFolder temporaryFolder)
        {
            Dictionary<string, object> storage = null;

            await temporaryFolder.GetFileAsync(fileName).AsTask().ContinueWith(item =>
            {
                if (!item.IsFaulted)
                {
                    var file = item.Result;
                    using (var content = file.OpenStreamForReadAsync())
                    using (var sr = new StreamReader(content.Result))
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        var d = (Dictionary<string, object>)Newtonsoft.Json.JsonConvert.DeserializeObject(sr.ReadToEnd(), new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All
                        });
                        storage = d;
                    }
                }
            });
            return storage;
        }
    }
}