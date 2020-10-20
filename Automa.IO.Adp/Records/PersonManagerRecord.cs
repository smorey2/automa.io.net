using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Adp.Records
{
    public class PersonManagerRecord
    {
        public string PersonId { get; set; }
        public string WorkEmail { get; set; }
        public string ManagerId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? EndDate { get; set; }

        public static async Task<IEnumerable<PersonManagerRecord>> GetPersonManagersAsync(AdpClient adp)
        {
            using (var stream = new MemoryStream())
            {
                var file = await adp.GetSingleReportAsync(AdpClient.ReportType.Custom, "Manager Report", AdpClient.ReportOptions.AllRecords, stream, deleteAfter: true);
                return CsvReader.Read(stream, x => new PersonManagerRecord
                {
                    PersonId = x[0],
                    WorkEmail = x[1],
                    ManagerId = x[2],
                    Date = !string.IsNullOrEmpty(x[3]) ? (DateTime?)DateTime.Parse(x[3]) : null,
                    EndDate = !string.IsNullOrEmpty(x[4]) ? (DateTime?)DateTime.Parse(x[4]) : null,
                }).ToList();
            }
        }
    }
}