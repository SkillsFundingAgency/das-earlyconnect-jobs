using System;
using System.Collections.Generic;
using SFA.DAS.EarlyConnect.Models.Interfaces.StudentData;

namespace SFA.DAS.EarlyConnect.Models.StudentData
{
    public class StudentDataList : IStudentDataList
    {
        public IEnumerable<StudentData> ListOfStudentData { get; set; }
    }
    public class StudentData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Postcode { get; set; }
        public string Industry { get; set; }
        public DateTime DateOfInterest { get; set; }
    }
}
