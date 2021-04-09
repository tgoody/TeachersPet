using System;

namespace TeachersPet.Services {
    public class AuthenticationProvider {

        private static AuthenticationProvider _instance;
        private AuthenticationProvider() {}
        public static AuthenticationProvider Instance() {
            return _instance ??= new AuthenticationProvider();
        }

        private string apiToken;

        public string ApiToken {
            get {
                if (apiToken != "") {
                    return apiToken;
                }
                throw new Exception("API Token Missing");
            }
        }


        public void CanvasLogin() {
            //TODO: Implement OAUTH here, use HTTP Listeners
            apiToken = RetrieveAPITokenFromEnvironment(); //This will change to be set to OAuth info.
            CanvasAPI.SetBearerToken(apiToken);
            CacheService.LoadLocalCache();
        }


        private string RetrieveAPITokenFromEnvironment() {
            return Environment.GetEnvironmentVariable("CanvasAPIToken", EnvironmentVariableTarget.User);
        }

    }
}