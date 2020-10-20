using Bogus;
using ExcelTrans.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Adp.Records
{
    public class PersonRecord
    {
        public bool Disabled { get; set; }
        public string Id { get; set; }
        public string Tag { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PreferredFirstName { get; set; }
        public string PreferredLastName { get; set; }
        public string Gender { get; set; }
        public string Race { get; set; }
        public string SSN4_EIN { get; set; }
        public string WorkEmail { get; set; }
        public string PersonalEmail { get; set; }
        public string OfficePhone { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string PrimaryStreet { get; set; }
        public string PrimaryStreet2 { get; set; }
        public string PrimaryCity { get; set; }
        public string PrimaryState { get; set; }
        public string PrimaryPostalCode { get; set; }
        public string PrimaryCountry { get; set; }
        public string Timezone { get; set; }
        public string UserPrincipalName { get; set; }
        public string Birthdate { get; set; }
        public string PositionId { get; set; }
        public string ManagerId { get; set; }
        public string EmergencyName { get; set; }
        public string EmergencyRelationship { get; set; }
        public string EmergencyPhone { get; set; }
        public string EmergencyEmail { get; set; }
        public string ExemptStatus { get; set; }

        public static async Task<IEnumerable<PersonRecord>> GetPeopleAsync(AdpClient adp, bool bogus)
        {
            var faker = bogus ? new Faker() : null;
            using (var stream = new MemoryStream())
            {
                var file = await adp.GetSingleReportAsync(AdpClient.ReportType.Custom, "People Report", AdpClient.ReportOptions.Standard, stream, deleteAfter: true);
                return CsvReader.Read(stream, x => new PersonRecord
                {
                    Tag = x[21],
                    Disabled = !string.IsNullOrEmpty(x[31]),
                    Id = x[26],
                    FirstName = x[4],
                    LastName = x[5],
                    MiddleName = x[2],
                    PreferredFirstName = x[0],
                    PreferredLastName = x[1],
                    Gender = x[6],
                    Race = x[7],
                    SSN4_EIN = !string.IsNullOrEmpty(x[8]) ? faker?.Random.Number(9999).ToString().PadLeft(4, '0') ?? ((int)decimal.Parse(x[8])).ToString("0000") : null,
                    WorkEmail = x[9],
                    PersonalEmail = faker?.Internet.Email() ?? x[10],
                    OfficePhone = x[11],
                    HomePhone = faker?.Phone.PhoneNumber() ?? x[12],
                    MobilePhone = faker?.Phone.PhoneNumber() ?? x[13],
                    PrimaryStreet = faker?.Address.StreetAddress() ?? x[14],
                    PrimaryStreet2 = faker != null ? null : x[15],
                    PrimaryCity = faker?.Address.City() ?? x[16],
                    PrimaryState = faker?.Address.StateAbbr() ?? x[17],
                    PrimaryPostalCode = faker?.Address.ZipCode() ?? x[18],
                    PrimaryCountry = x[19],
                    Timezone = null,
                    UserPrincipalName = x[9],
                    Birthdate = x[20] != null ? DateTime.Parse(x[20]).AddYears(-DateTime.Parse(x[20]).Year + 2000).ToString() : null,
                    PositionId = x[28],
                    ManagerId = x[27],
                    EmergencyName = faker.Name.FullName() ?? x[22],
                    EmergencyRelationship = faker != null ? "SomeRelation" : x[23],
                    EmergencyPhone = faker?.Phone.PhoneNumber() ?? x[24],
                    EmergencyEmail = faker?.Internet.Email() ?? x[25],
                    ExemptStatus = x[29],
                }).ToList();
            }
        }
    }
}