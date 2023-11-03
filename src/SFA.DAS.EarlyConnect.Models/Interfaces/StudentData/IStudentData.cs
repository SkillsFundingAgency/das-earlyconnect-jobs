using System;
using System.Collections.Generic;

namespace SFA.DAS.EarlyConnect.Models.Interfaces.StudentData
{
    public interface IStudentDataList
    {
        IEnumerable<Models.StudentData.StudentData> ListOfStudentData { get; set; }
    }

    public interface IStudentData
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        DateTime DateOfBirth { get; set; }
        string Email { get; set; }
        string Postcode { get; set; }
        string Industry { get; set; }
        DateTime DateOfInterest { get; set; }
    }
}
