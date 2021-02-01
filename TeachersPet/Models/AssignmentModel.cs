using System;

namespace TeachersPet.Models {
    public class AssignmentModel {
        public string Id {
            get => id;
            set => id = value;
        }

        public string Name {
            get => name;
            set => name = value;
        }

        public string Description {
            get => description;
            set => description = value;
        }

        public DateTime DueDate {
            get => dueDate;
            set => dueDate = value;
        }

        public string PointsPossible {
            get => points_possible;
            set => points_possible = value;
        }

        private string id;
        private string name;
        private string description;
        private DateTime dueDate;
        private string points_possible;

    }
}