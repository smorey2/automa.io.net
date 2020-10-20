using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Adp.Records
{
    public class PersonPositionRecord
    {
        public string PersonId { get; set; }
        public string WorkEmail { get; set; }
        public DateTime Date { get; set; }
        public string PositionId { get; set; }
        public string LocationId { get; set; }
        public string DepartmentId { get; set; }
        public decimal? ScheduledHours { get; set; }

        public static async Task<IEnumerable<PersonPositionRecord>> GetPersonPositionsAsync(AdpClient adp)
        {
            using (var stream = new MemoryStream())
            {
                var file = await adp.GetSingleReportAsync(AdpClient.ReportType.Custom, "Position Report", AdpClient.ReportOptions.AllRecords, stream, deleteAfter: true);
                return CsvReader.Read(stream, x => new PersonPositionRecord
                {
                    PersonId = x[0],
                    WorkEmail = x[1],
                    Date = DateTime.Parse(x[2]),
                    PositionId = x[3] ?? string.Empty,
                    LocationId = x[5] ?? string.Empty,
                    DepartmentId = x[7] ?? string.Empty,
                    ScheduledHours = !string.IsNullOrEmpty(x[9]) ? (decimal?)decimal.Parse(x[9]) : null,
                }).ToList();
            }
        }
    }
}