using System;

namespace API.Shared.Events
{
    public class JobAddedEvent : IGoEvent
    {
        public JobAddedEvent(string jobName) 
        { 
            JobId = Guid.NewGuid();
            JobName = jobName;
            CreatedAtUTC = DateTime.UtcNow;
        }

        public Guid JobId { get; private set; }
        public string JobName { get; private set; }
        public DateTime CreatedAtUTC { get; private set; }
    }
}
