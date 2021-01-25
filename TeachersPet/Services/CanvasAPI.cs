using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeachersPet.Services {
    
    //TODO: Make this guy a singleton
    
    public static class CanvasAPI {
        private static HttpClient httpClient = new HttpClient();
        private static string canvasAPIUrl;
        private static bool betaMode;

        
        static CanvasAPI() {
            canvasAPIUrl = App.Current.Properties["CanvasAPIUrl"] as string;
        }

        public static bool ToggleBetaCanvasMode() {
            //maybe make this an if statement if it's too gross
            var inProduction = canvasAPIUrl == App.Current.Properties["CanvasAPIUrl"] as string;
            canvasAPIUrl = (inProduction)
                ? App.Current.Properties["BetaCanvasAPIUrl"] as string
                : App.Current.Properties["CanvasAPIUrl"] as string;
            return !inProduction;
        }

        public static void SetBearerToken(string token) {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        
        
        private static async Task<JToken> CanvasApiRequest(string urlParameters) {
            var response = httpClient.GetAsync(canvasAPIUrl + urlParameters).Result;
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JToken>(content);
        }
        
        //TODO: lets find some way to store data program wide
        public static async Task<JArray> GetCourseList() {

            //If a teacher, use all courses where you are a teacher
            //else, you will get no results, so you must be a TA, so only show  courses where you are a TA
            var result = await CanvasApiRequest("courses?enrollment_type=teacher");
            if(!result.HasValues)
                result = await CanvasApiRequest("courses?enrollment_type=ta");
            try {
                foreach (var course in (JArray) result) {
                    Console.WriteLine((string) course["id"]);
                }
            }
            catch (Exception e) {
                throw new Exception("Error retrieving courses for your Canvas account");
            }

            return (JArray) result;
        }
        
            
        
        
        
    }
}