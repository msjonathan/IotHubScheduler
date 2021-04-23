using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTHubScheduler.API.Models
{
    public class CreateJobRequest
    {
        public string Query { get; set; }
        public string Data { get; set; }
        public JobType Type { get; set; }
    }

    public enum JobType
    {
        DirectMethod, 
        UpdateDeviceTwin
    }
}
