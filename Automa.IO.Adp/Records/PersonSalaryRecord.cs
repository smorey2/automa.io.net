using Bogus;
using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Adp.Records
{
    public class PersonSalaryRecord
    {
        public string PersonId { get; set; }
        public string WorkEmail { get; set; }

        public class SalaryRecord
        {
            public string PersonId { get; set; }
            public string WorkEmail { get; set; }
            //
            public DateTime Date { get; set; }
            public string RateType { get; set; }
            public string Frequency { get; set; }
            public decimal? Amount { get; set; }
            public decimal? AnnualAmount { get; set; }
            public string Reason { get; set; }
        }

        public static async Task<IEnumerable<PersonSalaryRecord>> GetPersonSalariesAsync(AdpClient adp, bool allRecords, bool bogus, Func<IEnumerable<SalaryRecord>, IEnumerable<PersonSalaryRecord>> secure)
        {
            var faker = new Faker();
            using (var stream = new MemoryStream())
            {
                var range = allRecords
                    ? AdpClient.ReportOptions.AllRecords
                    : AdpClient.ReportOptions.Standard;
                var file = await adp.GetSingleReportAsync(AdpClient.ReportType.Custom, "Salary Report", range, stream, deleteAfter: true);
                var salaries = CsvReader.Read(stream, x => new SalaryRecord
                {
                    PersonId = x[0],
                    WorkEmail = x[1],
                    Date = DateTime.Parse(x[2]),
                    RateType = x[6],
                    Frequency = x[4],
                    Amount = !string.IsNullOrEmpty(x[5]) ? (decimal?)decimal.Parse(x[5], NumberStyles.Currency) : null,
                    AnnualAmount = !string.IsNullOrEmpty(x[7]) ? (decimal?)decimal.Parse(x[7], NumberStyles.Currency) : null,
                    Reason = x[8],
                }).Where(x => x != null).ToList();
                if (bogus)
                {
                    var bogusValues = salaries.Select(x => x.Amount).Union(salaries.Select(x => x.AnnualAmount)).Where(x => x != null).GroupBy(x => x)
                        .ToDictionary(x => x.Key, y => faker.Random.Decimal(10, 250000));
                    foreach (var salary in salaries)
                    {
                        if (salary.Amount != null && bogusValues.TryGetValue(salary.Amount, out var z)) salary.Amount = z;
                        if (salary.AnnualAmount != null && bogusValues.TryGetValue(salary.AnnualAmount, out z)) salary.AnnualAmount = z;
                    }
                }
                return secure(salaries);
            }
        }
    }
}