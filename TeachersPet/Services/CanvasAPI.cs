using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeachersPet.Models;

namespace TeachersPet.Services {
    
    //TODO: Make this guy a singleton
    
    public static class CanvasAPI {
        private static HttpClient httpClient = new HttpClient();
        private static string canvasAPIUrl;
        private static string bearerToken;
        public static string BearerToken => bearerToken;

        public static string CanvasApiUrl => canvasAPIUrl;
        
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
            bearerToken = token;
        }
        
        public static async Task<JArray> GetCourseList() {
            //If a teacher, use all courses where you are a teacher
            //else, you will get no results, so you must be a TA, so only show  courses where you are a TA
            var result = await GetCacheCanvasApiRequest("courses?enrollment_type=teacher");
            if(!result.HasValues)
                result = await GetCacheCanvasApiRequest("courses?enrollment_type=ta");
            return (JArray) result;
        }
        
        public static async Task<JArray> GetStudentListFromCourseId(string courseID) { //TODO: Figure out retrieving emails on student list page
            var result = await GetCacheCanvasApiRequest($"courses/{courseID}/users?enrollment_type[]=student&include[]=avatar_url&include[]=sis_user_id");
            return result as JArray;
        }

        public static async Task<JToken> GetStudentProfileFromStudentId(string studentId) {
            var result = await GetCacheCanvasApiRequest($"users/{studentId}/profile");
            return result;
        }

        public static async Task<JArray> GetAssignmentListFromCourseId(string courseId) {
            var result = await GetCacheCanvasApiRequest($"courses/{courseId}/assignments");
            return result as JArray;
        }

        public static async Task<JToken> GetSubmissionForAssignmentForStudent(string courseId, string assignmentId, string studentId) {
            var result = await GetCanvasApiRequest($"courses/{courseId}/assignments/{assignmentId}/submissions/{studentId}");
            return result;
        }
        public static async Task<JArray> GetSubmissionsFromCourseAndAssignmentId(string courseId, string assignmentId) {
            var result = await GetCanvasApiRequest($"courses/{courseId}/assignments/{assignmentId}/submissions");
            return result as JArray;
        }

        public static async Task<JArray> GetSubmissionsFromUserAndCourseId(string userId, string courseId) {
            var result = await GetCanvasApiRequest($"courses/{courseId}/students/submissions?student_ids[]={userId}&include[]=assignment");
            return result as JArray;
        }

        public static async Task<JArray> GetSectionsFromCourseId(string courseId) {
            var result = await GetCacheCanvasApiRequest($"courses/{courseId}/sections?include[]=students");
            return result as JArray;
        }

        public static async Task<JToken> UpdateGradeFromSubmissionModel(SubmissionModel submissionModel, string comment=null) {
            var urlPath =
                $"courses/{submissionModel.AssignmentModel.CourseId}/assignments/{submissionModel.AssignmentId}/submissions/{submissionModel.UserId}";
            var jsonData = new Dictionary<string, string> {
                {"submission[posted_grade]", submissionModel.Score},
                {"comment[text_comment]", comment}
            };
            var result = await PutCanvasApiRequest(urlPath, jsonData);
            //TODO: find better solution to handle this
            var updateTask = new Task<JToken>(() => GetSubmissionsFromUserAndCourseId(submissionModel.UserId, submissionModel.AssignmentModel.CourseId).Result);
            CacheService.Update(
                $"courses/{submissionModel.AssignmentModel.CourseId}/students/submissions?student_ids[]={submissionModel.UserId}&include[]=assignment",
                updateTask);
            return result;
        }
        
        public static async Task<JToken> UpdateGradesFromStudentModelsAndScores(Dictionary<StudentModel, string> userIdsAndScores, string courseId, string assignmentId) {
            var dataForJson = new Dictionary<string, string>();
            foreach (var (key, value) in userIdsAndScores) {
                dataForJson.Add($"grade_data[{key.Id}][posted_grade]", value);
                dataForJson.Add($"grade_data[{key.Id}][text_comment]", $"Autograded on: {DateTime.Now.Date:d}\nIf this grade is incorrect, please contact your TA.");
            }
            return PostCanvasApiRequest($"courses/{courseId}/assignments/{assignmentId}/submissions/update_grades", dataForJson).Result;
        }

        public static async Task<JToken> UpdateSingleGradeFromStudentModelAndScore(StudentModel student, string score,
            string courseId, string assignmentId) {
            var dataForJson = new Dictionary<string, string>();
                dataForJson.Add("submission[posted_grade]", score);
                dataForJson.Add("comment[text_comment]",
                    $"Autograded on: {DateTime.Now.Date:d}\nIf this grade is incorrect, please contact your TA.");
                return PutCanvasApiRequest($"courses/{courseId}/assignments/{assignmentId}/submissions/{student.Id}", dataForJson).Result;
        }

        public static async Task<JToken> RefreshProgress(JToken progressObject) {
            var url = (string)progressObject["url"];
            var progressRoute = url.Substring(url.IndexOf("progress"));
            return await GetCanvasApiRequest(progressRoute);
        }
        
        private static async Task<JToken> GetCacheCanvasApiRequest(string route) {
            var task = new Task<JToken>(() => GetCanvasApiRequest(route).Result);
            CacheService.Set(route, task);
            return await CacheService.Get(route);
        }
        
        private static async Task<JToken> GetCanvasApiRequest(string route) {
            Console.WriteLine(route);
            var response = httpClient.GetAsync(canvasAPIUrl + route).Result;
            if (response.Headers.Contains("Link")) {
                return await HandlePaginationRequest(route);
            }
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JToken>(content);
        }

        private static async Task<JToken> PutCanvasApiRequest(string urlParameters, Dictionary<string, string> jsonData) {
            var payload = new FormUrlEncodedContent(jsonData);
            var response = httpClient.PutAsync(canvasAPIUrl + urlParameters, payload).Result;
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        private static async Task<JToken> PostCanvasApiRequest(string urlParameters,
            Dictionary<string, string> jsonData) {
            var payload = new FormUrlEncodedContent(jsonData);
            var response = httpClient.PostAsync(canvasAPIUrl + urlParameters, payload).Result;
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }


        private static async Task<JArray> HandlePaginationRequest(string urlParameters) {
            //TODO: what if 100 is too much, Canvas says there's an unspecified limit
            var resultArray = new JArray();
            if (!urlParameters.Contains('?')) {
                urlParameters += '?';
            }
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