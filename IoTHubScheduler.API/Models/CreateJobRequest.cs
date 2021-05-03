using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace IoTHubScheduler.API.Models
{
    public class CreateJobRequest
    {
        public string Query { get; set; }
        public string Data { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public JobType Type { get; set; }
    }

    public enum JobType
    {
        [EnumMember(Value = "DirectMethod")]
        DirectMethod,
        [EnumMember(Value = "UpdateDeviceTwin")]
        UpdateDeviceTwin
    }
}
