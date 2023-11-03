using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SFA.DAS.EarlyConnect.Models.StudentData;
using SFA.DAS.EarlyConnect.Models.Validation;

namespace SFA.DAS.EarlyConnect.Application.Services
{
    public interface ICsvService
    {
        CsvValidationeResult Validate(StreamReader csvStream, IList<string> fields);

        Task<List<StudentData>> ParseCsvData(StreamReader streamReader);
        Task<IList<dynamic>> ConvertToList(StreamReader csvStream);
        int GetByteCount<T>(IList<T> leads);
        string ToCsv<T>(IList<T> leads);
        bool IsEmpty(StreamReader stream);
        bool HasData(StreamReader stream);
    }
}