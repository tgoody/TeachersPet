using System;
using Newtonsoft.Json;

namespace TeachersPet.Models {
    public class CourseModel {
        
        [JsonProperty("start_at")]
        public DateTime? StartDateTime {
            get => startDateTime;
            set {
                if (value != null) startDateTime = (DateTime) value;
                else StartDateTime = DateTime.MaxValue;
            }
        }

        [JsonProperty("total_students")]
        public int TotalStudents {
            get => totalStudents;
            set => totalStudents = value;
        }
        
        [JsonProperty("id")]
        public string Id {
            get => id;
            set => id = value;
        }

        [JsonProperty("course_code")]
        public string CourseCode {
            get => courseCode;
            set => courseCode = value;
        }
        
        [JsonProperty("name")]

        public string CourseName {
            get => courseName;
            set => courseName = value;
        }

        private string courseCode;
        private string courseName;
        private DateTime startDateTime;
        private int totalStudents;
        private string id;

    }
}