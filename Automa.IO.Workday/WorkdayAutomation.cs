namespace Automa.IO.Workday
{
    /// <summary>
    /// WorkdayAutomation
    /// </summary>
    public class WorkdayAutomation : Automation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkdayAutomation"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="automa">The automa.</param>
        public WorkdayAutomation(AutomaClient client, IAutoma automa) : base(client, automa) { }
    }
}
