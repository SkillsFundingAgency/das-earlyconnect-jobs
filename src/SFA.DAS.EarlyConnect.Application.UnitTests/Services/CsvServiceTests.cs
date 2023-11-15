using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EarlyConnect.Application.Services;
using SFA.DAS.EarlyConnect.Application.UnitTests.Helpers;

namespace SFA.DAS.EarlyConnect.Application.UnitTests.Services
{
    public class CsvServiceTests
    {
        private CsvService _service;

        private string _testCsvSmall = CsvTestHelper.GetValidCsv(10, "North East", "2022", "0_10_miles", "1", "8", "100", "1");
        private string _testCsvLarge = CsvTestHelper.GetValidCsv(20000, "North East", "2022", "0_10_miles", "1", "8", "100", "1");

        [SetUp]
        public void Arrange()
        {
            _service = new CsvService();
        }

        [Test]
        public async Task ConvertToList_When_Ten_Rows_Then_Ten_Objects_In_List()
        {

            IList<dynamic> result;

            using (var test_Stream = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(_testCsvSmall))))
            {
                result = await _service.ConvertToList(test_Stream);
            }

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(10);
        }

        [Test]
        public async Task ConvertToList_When_20k_Rows_Then_20k_Objects_In_List()
        {

            IList<dynamic> result;

            using (var test_Stream = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(_testCsvLarge))))
            {
                result = await _service.ConvertToList(test_Stream);
            }

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(20000);
        }

        [Test]
        public async Task ConvertToList_When_Converted_Then_All_Properties_Are_Populated()
        {

            IList<dynamic> result;

            using (var test_Stream = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(_testCsvSmall))))
            {
                result = await _service.ConvertToList(test_Stream);
            }

            result.Should().NotBeNullOrEmpty();


            var singleResult = result.FirstOrDefault();
            ((object)singleResult).Should().NotBeNull();
            ((string)singleResult.Region).Should().Be("North East");
            ((string)singleResult.Intended_uni_entry_year).Should().Be("2022");
            ((string)singleResult.Max_travel_distance).Should().Be("0_10_miles");
            ((string)singleResult.willing_to_relocate_flag).Should().Be("1");
            ((string)singleResult.Number_gcse_grade4).Should().Be("8");
            ((string)singleResult.Students).Should().Be("100");
            ((string)singleResult.Interested_in_transport_flag).Should().Be("1");
        }

    }
}

