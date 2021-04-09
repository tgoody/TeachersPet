using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeachersPet.Services {
    public class CacheService {

        private static Dictionary<string, Task<JToken>> _cache = new Dictionary<string, Task<JToken>>();
        
        public static async Task<JToken> Get(string route) {
            return !_cache.ContainsKey(route) ? null : _cache[route].Result;
        }

        public static void Set(string route, Task<JToken> token) {
            if (_cache.ContainsKey(route)) {
                return;
            }
            _cache.Add(route, token);
            token.Start();
        }

        public static void Update(string route, Task<JToken> token) {
            if (!_cache.ContainsKey(route)) {
                return;
            }
            _cache[route] = token;
            token.Start();
        }

        public static void LoadLocalCache() {
            var result = new Dictionary<string, JToken>();
            try {
                var temp = System.Reflection.Assembly.GetExecutingAssembly().Location;
                temp = Path.GetDirectoryName(temp);
                var data = File.ReadAllBytes($"{temp}/Cache/localCache.tpf");
                
                data = EncryptService.DecryptData(data, CanvasAPI.BearerToken);
                var memoryStream = new MemoryStream(data);
                
                var formatter = new BinaryFormatter();
                var tempResult = (string)formatter.Deserialize(memoryStream);
                result = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(tempResult);
            }
            catch (Exception) {
                // ignored
            }
            _cache = result.ToDictionary(x => x.Key, x => Task.FromResult(x.Value));
        }
        
        public static void SaveLocalCache() {
            try {
                if (CanvasAPI.BearerToken.Length < 1) {
                    return;
                }
                var memoryStream = new MemoryStream();
                var formatter = new BinaryFormatter();
                var dictionary = _cache.ToDictionary(x => x.Key, x => x.Value.Result);
                var str = JsonConvert.SerializeObject(dictionary);
                formatter.Serialize(memoryStream, str);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var bytes = new byte[memoryStream.Length];
                memoryStream.Read(bytes, 0, (int) memoryStream.Length);
                bytes = EncryptService.EncryptData(bytes, CanvasAPI.BearerToken);
                
                var temp = System.Reflection.Assembly.GetExecutingAssembly().Location;
                temp = Path.GetDirectoryName(temp);
                Directory.CreateDirectory($"{temp}/Cache");
                File.Delete($"{temp}/Cache/localCache.tpf");
                var fileStream = File.OpenWrite($"{temp}/Cache/localCache.tpf");
                fileStream.Write(bytes);
            }
            catch (Exception) {
                // ignored
            }
        }

        public static void ClearCache() {
            _cache = new Dictionary<string, Task<JToken>>();
            var temp = System.Reflection.Assembly.GetExecutingAssembly().Location;
            temp = Path.GetDirectoryName(temp);
            var cacheFile = new FileInfo($"{temp}/Cache/localCache.tpf");
            if (cacheFile.Exists) {
                File.Delete(cacheFile.FullName);
            }
        }
    }
}