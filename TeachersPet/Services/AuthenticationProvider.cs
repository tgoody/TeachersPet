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
            apiToken = RetrieveBetaAPITokenFromEnvironment(); //This will change to be set to OAuth info.
        }


        private string RetrieveAPITokenFromEnvironment() {
            return System.Environment.GetEnvironmentVariable("CanvasAPIToken", EnvironmentVariableTarget.User);
        }

        private string RetrieveBetaAPITokenFromEnvironment() {
            return System.Environment.GetEnvironmentVariable("BetaCanvasAPIToken", EnvironmentVariableTarget.User);
        }
        
    }
}