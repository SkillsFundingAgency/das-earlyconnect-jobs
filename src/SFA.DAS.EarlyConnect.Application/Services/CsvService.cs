using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using FormatValidator;
using SFA.DAS.EarlyConnect.Models.Validation;

namespace SFA.DAS.EarlyConnect.Application.Services
{
    public class CsvService : ICsvService
    {
        public CsvValidationeResult Validate(StreamReader csvStream, IList<string> fields)
        {
            var config = new ValidatorConfiguration();

            config.ColumnSeperator = ",";
            config.HasHeaderRow = true;
            config.RowSeperator = Environment.NewLine;

            for (int i = 0; i < fields.Count; i++)
            {
                config.Columns.Add(i, new ColumnValidatorConfiguration()
                {
                    Name = fields[i],
                    IsNumeric = false,
                    Unique = true
                });
            }

            Validator validator = Validator.FromConfiguration(config);
            var sourceReader = new StreamSourceReader(csvStream.BaseStream);

            var validationResult = new CsvValidationeResult
            {
                Errors = validator.Validate(sourceReader).ToList()
            };
            return validationResult;
        }

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

            //if there is data and not just headers, the second line should have data and shouldnt be whitespace

            var secondLine = stream.ReadLine();

            return String.IsNullOrWhiteSpace(secondLine) == false;
        }
    }
    public interface ICsvService
    {
        CsvValidationeResult Validate(StreamReader csvStream, IList<string> fields);
        Task<IList<dynamic>> ConvertToList(StreamReader csvStream);
        bool IsEmpty(StreamReader stream);
        bool HasData(StreamReader stream);
    }
}