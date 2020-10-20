using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Automa.IO.Adp.Records
{
    public class DepartmentRecord
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool Disabled { get; set; }

        public static async Task<IEnumerable<DepartmentRecord>> GetDepartmentsAsync(AdpClient adp) =>
            (await adp.GetValidationTablesAsync("department", "/BXC", x => new DepartmentRecord
            {
                Id = (string)x["code"],
                Title = ((string)x["description"]).Trim(),
                Disabled = !(bool)x["active"],
            })).ToList();
    }
}