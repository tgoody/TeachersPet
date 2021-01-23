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
        }


        private string RetrieveAPITokenFromEnvironment() {
            return System.Environment.GetEnvironmentVariable("CanvasAPIToken", EnvironmentVariableTarget.User);
        }

        //TODO: Figure out whether canvas API uses production tokens for beta URLs
        private string RetrieveBetaAPITokenFromEnvironment() {
            return System.Environment.GetEnvironmentVariable("BetaCanvasAPIToken", EnvironmentVariableTarget.User);
        }
        
    }
}