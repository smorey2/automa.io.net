using Bogus;
using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Adp.Records
{
    public class PersonEmploymentRecord
    {
        public string AssociateId { get; set; }
        public string Status { get; set; }
        public DateTime StatusEffectiveDate { get; set; }
        public DateTime? StatusEffectiveEndDate { get; set; }
        public DateTime? HireDate { get; set; }
        public string HireReason { get; set; }
        public DateTime? RehireDate { get; set; }
        public string RehireReason { get; set; }
        public DateTime? LoaStartDate { get; set; }
        public string LoaStartReason { get; set; }
        public DateTime? LoaReturnDate { get; set; }
        public string LoaReturnReason { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string TerminationReason { get; set; }
        public string PositionId { get; set; }
        public string LocationId { get; set; }
        public string WorkerCategory { get; set; }
        public string WorkEmail { get; set; }

        public static async Task<IEnumerable<PersonEmploymentRecord>> GetPersonEmploymentsAsync(AdpClient adp, bool bogus)
        {
            var faker = bogus ? new Faker() : null;
            using (var stream = new MemoryStream())
            {
                var file = await adp.GetSingleReportAsync(AdpClient.ReportType.Custom, "Employment Report", AdpClient.ReportOptions.AllRecords, stream, deleteAfter: true);
                return CsvReader.Read(stream, x => new PersonEmploymentRecord
                {
                    AssociateId = x[0],
                    Status = x[1] ?? string.Empty,
                    StatusEffectiveDate = DateTime.Parse(x[2]),
                    StatusEffectiveEndDate = string.IsNullOrWhiteSpace(x[3]) ? (DateTime?)null : DateTime.Parse(x[3]),
                    HireDate = string.IsNullOrWhiteSpace(x[4]) ? (DateTime?)null : DateTime.Parse(x[4]),
                    HireReason = x[5],
                    RehireDate = string.IsNullOrWhiteSpace(x[6]) ? (DateTime?)null : DateTime.Parse(x[6]),
                    RehireReason = x[7],
                    LoaStartDate = !string.IsNullOrEmpty(x[8]) ? (DateTime?)DateTime.Parse(x[8]) : null,
                    LoaStartReason = faker?.Lorem.Sentence() ?? x[9] ?? string.Empty,
                    LoaReturnDate = !string.IsNullOrEmpty(x[10]) ? (DateTime?)DateTime.Parse(x[10]) : null,
                    LoaReturnReason = faker?.Lorem.Sentence() ?? x[11] ?? string.Empty,
                    TerminationDate = !string.IsNullOrEmpty(x[12]) ? (DateTime?)DateTime.Parse(x[12]) : null,
                    TerminationReason = faker?.Lorem.Sentence() ?? x[13] ?? string.Empty,
                    WorkEmail = x[14] ?? string.Empty,
                    PositionId = x[15] ?? string.Empty,
                    LocationId = x[17] ?? string.Empty,
                    WorkerCategory = x[20] ?? string.Empty,
                }).ToList();
            }
        }
    }
}