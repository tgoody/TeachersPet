using Newtonsoft.Json;

namespace TeachersPet.Models {
    public class StudentModel {
        [JsonProperty("id")]

        public string Id {
            get => id;
            set => id = value;
        }
        [JsonProperty("name")]

        public string Name {
            get => name;
            set => name = value;
        }
        [JsonProperty("sis_user_id")]

        public int SisUserId {
            get => sisUserId;
            set => sisUserId = value;
        }

        [JsonProperty("email")]
        public string Email {
            get => email;
            set => email = value;
        }

        [JsonProperty("login_id")]
        private string LoginId {
            set => Email = value;
        }
        [JsonProperty("avatar_url")]

        public string AvatarUrl {
            get => avatarUrl;
            set => avatarUrl = value;
        }

        private string id;
        private string name;
        private int sisUserId; //UFID?
        private string email;
        private string avatarUrl; //image

        
    }
}