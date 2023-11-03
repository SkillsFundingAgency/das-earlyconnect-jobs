using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EarlyConnect.Models.Validation
{
    public class FieldValidationResult
    {
        public FieldValidationResult()
        {
            Errors = new List<string>();
        }
        public bool IsValid => Errors.Any() == false;
        public IList<string> Errors { get; set; }
    }
}