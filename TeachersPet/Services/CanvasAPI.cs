using System;
using System.Collections.Generic;
using System.Linq;
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
                throw new Exception($"Error retrieving courses for your Canvas account: {e.Message}");
            }
            return (JArray) result;
        }
        
        public static async Task<JArray> GetStudentListFromCourseId(string courseID) { //TODO: Figure out retrieving emails on student list page
            var result = await CanvasApiRequest($"courses/{courseID}/users?enrollment_type[]=student&include[]=avatar_url");
            return result as JArray;
        }

        public static async Task<JToken> GetStudentProfileFromStudentId(string studentId) {
            Console.WriteLine(studentId);
            var result = await CanvasApiRequest($"users/{studentId}/profile");
            return result;
        }
        
        
        
        
        private static async Task<JToken> CanvasApiRequest(string urlParameters) {
            var response = httpClient.GetAsync(canvasAPIUrl + urlParameters).Result;
            if (response.Headers.Contains("Link")) {
                return await HandlePaginationRequest(urlParameters);
            }
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JToken>(content);
        }



        private static async Task<JArray> HandlePaginationRequest(string urlParameters) {
            //TODO: what if 100 is too much, Canvas says there's an unspecified limit
            var resultArray = new JArray();
            urlParameters += "&per_page=100";
            var response = httpClient.GetAsync(canvasAPIUrl + urlParameters).Result;
            if (!response.Headers.Contains("Link")) {
                throw new Exception("Error reading paginated Canvas API response");
            }
            var values = response.Headers.GetValues("Link").ToList()[0];
            var links = values.Split(',');
            var nextLink = links.SingleOrDefault(s => s.Contains("rel=\"next\""))?.Split('<', '>')[1];
            var currentLink = links.SingleOrDefault(s => s.Contains("rel=\"current\""))?.Split('<', '>')[1];
            var lastLink = links.SingleOrDefault(s => s.Contains("rel=\"last\""))?.Split('<', '>')[1];
            
            var content = JsonConvert.DeserializeObject<JArray>(await response.Content.ReadAsStringAsync());
            resultArray.Merge(content);
            
            while (nextLink != null && currentLink != lastLink) {
                Console.WriteLine(resultArray.Count);
                response = httpClient.GetAsync(nextLink).Result;
                if (!response.Headers.Contains("Link")) {
                    throw new Exception("Error reading paginated Canvas API response");
                }
                values = response.Headers.GetValues("Link").ToList()[0];
                links = values.Split(',');
                content = JsonConvert.DeserializeObject<JArray>(await response.Content.ReadAsStringAsync());
                resultArray.Merge(content);
                nextLink = links.SingleOrDefault(s => s.Contains("rel=\"next\""))?.Split('<', '>')[1];
                currentLink = links.SingleOrDefault(s => s.Contains("rel=\"current\""))?.Split('<', '>')[1];
                lastLink = links.SingleOrDefault(s => s.Contains("rel=\"last\""))?.Split('<', '>')[1];

            }
            return resultArray;

        }
        
    }
}