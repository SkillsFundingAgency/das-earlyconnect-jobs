using SFA.DAS.EarlyConnect.Models.StudentData;

namespace SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Requests
{
    public class CreateStudentDataRequest : IPostApiRequest
    {
        public object Data { get; set; }
        
        public CreateStudentDataRequest(StudentDataList studentDataList)
        {
            Data = studentDataList;
        }

        public string PostUrl => $"CreateStudentData";
    }
}