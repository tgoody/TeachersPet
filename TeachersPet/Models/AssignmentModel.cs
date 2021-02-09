using System;
using System.Linq;
using Newtonsoft.Json;

namespace TeachersPet.Models {
    public class AssignmentModel {
        public string Id {
            get => _id;
            set => _id = value;
        }

        public string Name {
            get => _name;
            set => _name = value;
        }

        public string Description {
            get => _description;
            set => _description = value;
        }

        [JsonProperty("due_at")]
        public DateTime? DueDate {
            get => _dueDate;
            set => _dueDate = value;
        }

        public string DueDateString => _dueDate == null || !_dueDate.ToString().Trim().Any() ? "N/A" : _dueDate.Value.ToShortDateString();

        [JsonProperty("points_possible")]
        public string PointsPossible {
            get => _pointsPossible;
            set => _pointsPossible = value;
        }

        public string CourseId {
            get => courseID;
            set => courseID = value;
        }
        
        private string _id;
        private string _name;
        private string _description;
        private DateTime? _dueDate;
        private string _pointsPossible;
        private string courseID;

       
    }
}