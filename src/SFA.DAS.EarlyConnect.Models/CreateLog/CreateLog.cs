namespace SFA.DAS.EarlyConnect.Models.CreateLog
{
    public class CreateLog
    {
        public string RequestType { get; set; }
        public string RequestSource { get; set; }
        public string RequestIP { get; set; }
        public string Payload { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
    }
}
