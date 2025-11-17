using System;

namespace API.Shared.Events
{
    public class JobResultEvent : IGoEvent
    {
        public JobResultEvent(Guid jobId, string jobName, DateTime createdAtUTC, DateTime finishedAtUTC)
        {
            JobId = jobId;
            JobName = jobName;
            CreatedAtUTC = createdAtUTC;
            FinishedAtUTC = finishedAtUTC;
        }

        public Guid JobId { get; private set; }
        public string JobName { get; private set; }
        public DateTime CreatedAtUTC { get; private set; }
        public DateTime FinishedAtUTC { get; private set; }
    }
}
