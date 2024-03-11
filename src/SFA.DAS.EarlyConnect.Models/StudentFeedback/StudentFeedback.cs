using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.EarlyConnect.Models.StudentFeedback
{
    public class StudentFeedbackList
    {
        public IEnumerable<StudentFeedback> ListOfStudentFeedback { get; set; }
    }
    public class StudentFeedback
    {
        public Guid SurveyId { get; set; }
        public int LogId { get; set; }
        public string StatusUpdate { get; set; }
        public string Notes { get; set; }
        public string UpdatedBy { get; set; }
    }
}
