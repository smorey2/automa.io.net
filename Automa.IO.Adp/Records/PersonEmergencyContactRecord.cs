using Bogus;
using ExcelTrans.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Adp.Records
{
    public class PersonEmergencyContactRecord
    {
        public string AssociateId { get; set; }
        public string Primary { get; set; }
        public string EmergencyName { get; set; }
        public string EmergencyRelationship { get; set; }
        public string EmergencyPhone { get; set; }
        public string EmergencyEmail { get; set; }

        public static async Task<IEnumerable<PersonEmergencyContactRecord>> GetPersonEmergencyContactsAsync(AdpClient adp, bool bogus)
        {
            var faker = bogus ? new Faker() : null;
            using (var stream = new MemoryStream())
            {
                var file = await adp.GetSingleReportAsync(AdpClient.ReportType.Custom, "Emergency Contact Report", AdpClient.ReportOptions.AllRecords, stream, deleteAfter: true);
                return CsvReader.Read(stream, x => new PersonEmergencyContactRecord
                {
                    AssociateId = x[5],
                    Primary = x[0],
                    EmergencyName = faker?.Name.FullName() ?? x[1],
                    EmergencyRelationship = faker != null ? "SomeRelation" : x[2],
                    EmergencyPhone = faker?.Phone.PhoneNumber() ?? x[3],
                    EmergencyEmail = faker?.Internet.Email() ?? x[4],
                }).ToList();
            }
        }
    }
}