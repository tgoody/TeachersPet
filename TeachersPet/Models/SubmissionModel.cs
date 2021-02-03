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
            get => score ?? "N/A";
            set => score = value;
        }

        [JsonProperty("assignment")]
        public AssignmentModel AssignmentModel {
            get => _assignmentModel;
            set => _assignmentModel = value;
        }

        private string assignmentId;
        private string grade;
        private string score;
        private AssignmentModel _assignmentModel;
    }
}