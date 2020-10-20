using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Automa.IO.Workday.Records
{
    public class PersonRecord
    {
        public string Id { get; set; }
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
        public string UserPrincipalName { get; set; }
        public string Birthdate { get; set; }

        public static async Task<IEnumerable<PersonRecord>> GetPeopleAsync(WorkdayClient workday)
        {
            var file = await workday.GetReportAsync("/d/task/1422$2059.htmld");
            throw new NotImplementedException();
        }

        //static void PreferredName(string v, out string preferredFirstName, out string preferredLastName)
        //{
        //    var parts = v != null && v.Contains(" ") ? v.Split(new[] { ' ' }, 2) : null;
        //    preferredFirstName = parts?[0];
        //    preferredLastName = parts?[1];
        //}
    }
}
