using SFA.DAS.EarlyConnect.Models.StudentFeedback;

namespace SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests
{
    public  class CreateStudentFeedbackRequest : IPostApiRequest
    {
        public object Data { get; set; }

        public CreateStudentFeedbackRequest(StudentFeedbackList studentFeedbackList)
        {
            Data = studentFeedbackList;
        }

        public string PostUrl => "leps-data/student-feedback";
    }
}
