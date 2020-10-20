using System.Collections.Generic;
using System.Threading.Tasks;

namespace Automa.IO.Adp.Records
{
    public class PositionRecord
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool Disabled { get; set; }

        public static async Task<IEnumerable<PositionRecord>> GetPositionsAsync(AdpClient adp) =>
            await adp.GetValidationTablesAsync("jobtitle", "/allData", x => new PositionRecord
            {
                Id = (string)x["code"],
                Title = ((string)x["description"])?.Trim(),
                Disabled = !(bool)x["active"],
            });
    }
}