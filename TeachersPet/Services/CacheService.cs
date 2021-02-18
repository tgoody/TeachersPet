using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TeachersPet.Services {
    public class CacheService {

        private static Dictionary<string, Task<JToken>> _cache = new Dictionary<string, Task<JToken>>();
        
        public static async Task<JToken> Get(string route) {
            return !_cache.ContainsKey(route) ? null : _cache[route].Result;
        }

        public static void Set(string route, Task<JToken> token) {
            Console.WriteLine($"In cache for {route}");
            if (_cache.ContainsKey(route)) {
                return;
            }
            _cache.Add(route, token);
            token.Start();
        }
        

    }
}