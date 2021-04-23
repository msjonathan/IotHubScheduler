using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTHubScheduler.API.Models
{
    /// <summary>
    /// For poc sakes, I use only 1 model atm
    /// </summary>
    public class Job
    {
        public string Id { get; set; }
        public string Query { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
