using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;

namespace SFA.DAS.EarlyConnect.Application.Services
{
    public class CsvService : ICsvService
    {
        public async Task<IList<dynamic>> ConvertToList(StreamReader personCsv)
        {
            personCsv.DiscardBufferedData();
            personCsv.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

            using (var csv = new CsvReader(personCsv, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<dynamic>();
                return records.ToList<dynamic>();
            }
        }

        public bool IsEmpty(StreamReader stream)
        {

            stream.DiscardBufferedData();
            stream.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

            if (stream.BaseStream.Length < 2)
            {
                return true;
            }

            return String.IsNullOrWhiteSpace(stream.Peek().ToString());
        }

        public bool HasData(StreamReader stream)
        {
            stream.DiscardBufferedData();
            stream.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.ReadLine();

            var secondLine = stream.ReadLine();

            return String.IsNullOrWhiteSpace(secondLine) == false;
        }
    }
    public interface ICsvService
    {
        Task<IList<dynamic>> ConvertToList(StreamReader csvStream);
        bool IsEmpty(StreamReader stream);
        bool HasData(StreamReader stream);
    }
}