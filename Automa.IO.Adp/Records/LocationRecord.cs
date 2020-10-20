using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Automa.IO.Adp.Records
{
    public class LocationRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Disabled { get; set; }

        public static async Task<IEnumerable<LocationRecord>> GetLocationsAsync(AdpClient adp) =>
            (await adp.GetValidationTablesAsync("location", null, x => new LocationRecord
            {
                Id = (string)x["code"],
                Name = ((string)x["description"]).Trim(),
                Disabled = !(bool)x["active"],
            })).ToList();
    }
}