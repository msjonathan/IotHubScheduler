using IoTJobClient.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl;

namespace IoTJobClient.Data
{
    public class IotJobService
    {
        public readonly string _jobUrl;
        public IotJobService(IConfiguration configuration)
        {
            _jobUrl = configuration.GetValue<string>("JobUrl");
        }
        public async Task<List<Job>> GetJobs()
        {
            return await _jobUrl.GetJsonAsync<List<Job>>();
        }
    }
}
