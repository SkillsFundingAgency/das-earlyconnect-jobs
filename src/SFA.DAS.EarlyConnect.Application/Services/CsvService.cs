using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using FormatValidator;
using SFA.DAS.EarlyConnect.Models.StudentData;
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

        public async Task<List<StudentData>> ParseCsvData(StreamReader streamReader)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true, // Set this to true if your CSV has a header row
                BadDataFound = null // Optionally handle bad data
            };

            using (var csv = new CsvReader(streamReader, config))
            {
                csv.Context.RegisterClassMap<StudentDataMap>();
                return csv.GetRecords<StudentData>().ToList();
            }
        }

        public int GetByteCount<T>(IList<T> leads)
        {
            var csvString = ToCsv(leads);

            return System.Text.Encoding.Unicode.GetByteCount(csvString);
        }

        public string ToCsv<T>(IList<T> leads)
        {

            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<StudentDataMap>();

                csv.WriteRecords(leads);

                writer.Flush();
                return writer.ToString();
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


        //public sealed class StudentDataMap : ClassMap<StudentData>
        public class StudentDataMap : ClassMap<StudentData>
        {
            public StudentDataMap()
            {
                Map(m => m.FirstName).Name("firstName").Optional();
                Map(m => m.LastName).Name("lastName").Optional();
                //Map(m => m.DateOfBirth).ConvertUsing(row => ParseDateOfBirth(row.GetField("age")));
                Map(m => m.Email).Name("email").Optional();
                Map(m => m.Postcode).Name("postcode").Optional();
                Map(m => m.Industry).Name("industrydescription").Optional();
                //Map(m => m.DateOfInterest).ConvertUsing(row => ParseDateOfInterest(row.GetField("ExtractDate")));
            }

            private DateTime ParseDateOfBirth(string age)
            {
                if (int.TryParse(age, out int ageValue))
                {
                    return DateTime.Now.AddYears(-ageValue);
                }
                return DateTime.MinValue;
            }

            private DateTime ParseDateOfInterest(string extractDate)
            {
                if (DateTime.TryParse(extractDate, out DateTime date))
                {
                    return date;
                }
                return DateTime.MinValue;
            }
        }
    }
}