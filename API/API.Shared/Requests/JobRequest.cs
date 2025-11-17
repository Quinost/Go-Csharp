using System;

namespace API.Shared.Requests
{
    public class JobRequest
    {
        public JobRequest(string data, string data2, int data3) 
        {
            Id = Guid.NewGuid();
            Data = data;
            Data2 = data2;
            Data3 = data3;
        }

        public Guid Id { get; }
        public string Data { get; set; }
        public string Data2 { get; set; }
        public int Data3 { get; set; }
    }
}
