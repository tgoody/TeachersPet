using System;
using Newtonsoft.Json;

namespace TeachersPet.Models {
    public class SubmissionModel {
        [JsonProperty("assignment_id")]
        public string AssignmentId {
            get => assignmentId;
            set => assignmentId = value;
        }
        [JsonProperty("grade")]
        public string Grade {
            get => grade ?? "N/A";
            set => grade = value;
        }

        [JsonProperty("score")]
        public string Score {
            get {
                if (string.IsNullOrEmpty(score)) {
                    return "N/A";
                }
                return Math.Round(Convert.ToDecimal(score), 2).ToString();
            }
            set => score = value;
        }

        [JsonProperty("assignment")]
        public AssignmentModel AssignmentModel {
            get => _assignmentModel;
            set => _assignmentModel = value;
        }
        [JsonProperty("user_id")]
        public string UserId {
            get => userId;
            set => userId = value;
        }

        private string assignmentId;
        private string grade;
        private string score;
        private string userId;
        

        private AssignmentModel _assignmentModel;
    }
}