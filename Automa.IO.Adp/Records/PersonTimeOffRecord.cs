using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Adp.Records
{
    public class PersonTimeOffRecord
    {
        public string AssociateId { get; set; }
        public string WorkEmail { get; set; }
        public DateTime Date { get; set; }
        public string Duration { get; set; }
        public string AbsenceType { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedDate { get; set; }

        public static async Task<IEnumerable<PersonTimeOffRecord>> GetPersonTimeOffsAsync(AdpClient adp)
        {
            using (var stream = new MemoryStream())
            {
                var file = await adp.GetSingleReportAsync(AdpClient.ReportType.Custom, "Time Off", AdpClient.ReportOptions.Standard, stream, deleteAfter: true);
                return CsvReader.Read(stream, x => new PersonTimeOffRecord
                {
                    AssociateId = x[0],
                    WorkEmail = x[1],
                    Date = DateTime.TryParse($"{x[2]} {x[3]}", out var date) ? date : throw new InvalidOperationException("Report must return only records with data."),
                    Duration = x[4],
                    AbsenceType = x[5],
                    Status = x[6],
                    SubmittedDate = DateTime.Parse(x[7]),
                }).ToList();
            }
        }
    }
}