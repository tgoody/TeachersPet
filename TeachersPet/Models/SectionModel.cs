using System.Collections.Generic;
using Newtonsoft.Json;

namespace TeachersPet.Models {
    public class SectionModel {
        public string Name {
            get => name;
            set => name = value;
        }

        public string Id {
            get => id;
            set => id = value;
        }

        [JsonProperty("course_id")]
        public string CourseId {
            get => courseId;
            set => courseId = value;
        }

        public List<StudentModel> Students {
            get => _students;
            set => _students = value;
        }

        private string name;
        private string id;
        private string courseId;
        private List<StudentModel> _students;
        
    }
}