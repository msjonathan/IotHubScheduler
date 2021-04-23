using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTJobClient.Models
{
    public class Job
    {
        public string Id { get; set; }
        public string Query { get; set; }
        public string Type { get; set; }
        public DateTime CreationTime { get; set; }
    }

}
