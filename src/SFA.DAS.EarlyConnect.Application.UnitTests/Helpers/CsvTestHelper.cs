using System.Text;

namespace SFA.DAS.EarlyConnect.Application.UnitTests.Helpers
{
    public class CsvTestHelper
    {
        public static string GetValidCsv(int count, string region, string intended_uni_entry_year, string max_travel_distance, string willing_to_relocate_flag, string number_gcse_grade4, string students, string interested_in_transport_flag)
        {

            var csv = new StringBuilder()
                .AppendLine($"Region,Intended_uni_entry_year,Max_travel_distance,Willing_to_relocate_flag,Number_gcse_grade4,Students,Interested_in_transport_flag");

            for (int i = 1; i < count + 1; i++)
            {

                csv.AppendLine($"{region},{intended_uni_entry_year},{max_travel_distance},{willing_to_relocate_flag},{number_gcse_grade4},{students},{interested_in_transport_flag}");
            }

            return csv.ToString();
        }
    }
}