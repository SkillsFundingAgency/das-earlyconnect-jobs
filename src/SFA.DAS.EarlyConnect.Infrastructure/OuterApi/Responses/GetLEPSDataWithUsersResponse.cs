using System;
using System.Collections.Generic;

namespace SFA.DAS.EarlyConnect.Infrastructure.OuterApi.Responses
{
    public class GetLEPSDataWithUsersResponse
    {
        public int Id { get; set; }
        public string LepCode { get; set; }
        public string Region { get; set; }
        public string LepName { get; set; }
        public string EntityEmail { get; set; }
        public string Postcode { get; set; }
        public DateTime DateAdded { get; set; }
        public ICollection<LEPSUserResponse> Users { get; set; }
    }

    public class LEPSUserResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime DateAdded { get; set; }
    }
}

