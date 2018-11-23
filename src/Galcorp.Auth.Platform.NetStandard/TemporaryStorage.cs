using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Galcorp.Auth.Platform.NetStandard
{
    public class TemporaryStorage : IStore
    {
        private readonly string fileName = "galcorp.auth.cache.json";

        public Task<T> Read<T>(string key)
            where T: class
        {
            var temporaryFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var name = Path.Combine(temporaryFolder, fileName);

            Dictionary<string, object> storage = null;
            if(File.Exists(name)){
                using (var content = File.OpenRead(name))
                using (var sr = new StreamReader(content))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    storage = new JsonSerializer().Deserialize<Dictionary<string, object>>(jsonTextReader);
                }

                if(storage.ContainsKey(key))
                    return Task.FromResult((T)storage[key]);
            }
            
            return Task.FromResult((T)null);
        }

        public Task Store(string key, object value)
        {
            var temporaryFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var name = Path.Combine(temporaryFolder, fileName);

            Dictionary<string, object> storage = null;

            using (var content = File.OpenRead(name))
            using (var sr = new StreamReader(content))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                storage = new JsonSerializer().Deserialize<Dictionary<string, object>>(jsonTextReader);
            }

            if (storage == null)
                storage = new Dictionary<string, object>();

            if (!storage.ContainsKey(key))
                storage.Add(key, value);
            else
                storage[key] = value;

            var text = JsonConvert.SerializeObject(storage, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            File.WriteAllText(name, text);

            return Task.CompletedTask;
        }
    }
}