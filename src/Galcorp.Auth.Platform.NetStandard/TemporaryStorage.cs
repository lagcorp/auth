﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Galcorp.Auth.Platform.NetStandard
{
    public class TemporaryStorage : IStore
    {
        private readonly string fileName = "galcorp.auth.cache.json";
        private string _appNamePrefix;

        public TemporaryStorage(string appNamePrefix)
        {
            _appNamePrefix = appNamePrefix;
        }

        public Dictionary<string, object> GetCache(string name)
        {
            if(File.Exists(name)){
                using (var content = File.OpenRead(name))
                using (var sr = new StreamReader(content))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    var d = Newtonsoft.Json.JsonConvert.DeserializeObject(sr.ReadToEnd(), new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

                    return (Dictionary<string, object> )d;
                }
            }

            return null;
        }

        public Task<T> Read<T>(string key)
            where T: class
        {
            var temporaryFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var name = Path.Combine(temporaryFolder, _appNamePrefix?? "",fileName);

            var storage = GetCache(name);
            if(storage !=null && storage.ContainsKey(key))
            {
                var data = storage[key];

                return Task.FromResult((T)data);
            }
        
            return Task.FromResult((T)null);
        }

        public Task Store(string key, object value)
        {
            var temporaryFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var name = Path.Combine(temporaryFolder, _appNamePrefix?? "",fileName);

            var storage = GetCache(name);

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
            if (!Directory.Exists(Path.Combine(temporaryFolder, _appNamePrefix ?? "")))
            {
                Directory.CreateDirectory(Path.Combine(temporaryFolder, _appNamePrefix ?? ""));
            }
            File.WriteAllText(name, text);

            return Task.CompletedTask;
        }
    }
}