using Automa.IO.Okta;
using System.Net.Http;
using System.Threading.Tasks;

namespace Automa.IO.Workday
{
    /// <summary>
    /// WorkdayClient
    /// </summary>
    public partial class WorkdayClient : AutomaClient
    {
        const string WorkdayUri = "https://wd3.myworkday.com/dentsuaegis";
        readonly OktaClient _okta;
        readonly string _workdayId;

        public WorkdayClient(string workdayId)
            : base(client => new Automa(client, automa => new WorkdayAutomation(client, automa))) => _workdayId = workdayId;
        public WorkdayClient(OktaClient okta, string workdayId)
            : base(client => new Automa(client, automa => new WorkdayAutomation(client, automa)))
        {
            _okta = okta;
            _workdayId = workdayId;
        }

        public override bool EnsureAccess(AccessMethod method, AccessMode mode, ref object tag, object value = null) =>
            mode == AccessMode.Request && ((string)value).IndexOf("/authgwy/dentsuaegis/login.htmld") != -1;

        public async Task<object> GetReportAsync(string url)
        {
            var d0 = await this.TryFuncAsync(() => this.DownloadDataAsync(HttpMethod.Get, $"{WorkdayUri}/{_workdayId}/d/home.htmld"));
            return null;
        }
    }
}
