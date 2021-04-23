using IoTHubScheduler.API.Models;
using IoTHubScheduler.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTHubScheduler.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly ILogger<JobsController> _logger;
        private JobClient _jobClient;
        private IJobStorage _jobStorage;

        public JobsController(ILogger<JobsController> logger, IConfiguration configuration, IJobStorage jobStorage)
        {
            _logger = logger;
            _jobClient = JobClient.CreateFromConnectionString(configuration.GetConnectionString("iothub"));
            _jobStorage = jobStorage;
        }

        [HttpGet]
        public async Task<IEnumerable<Job>> GetJobs()
        {
            var persistedJobs = await _jobStorage.FetchAllJobs();

            await _jobClient.OpenAsync();

            foreach (var job in persistedJobs)
            {
                var test = await _jobClient.GetJobAsync(job.Id);
            }

            await _jobClient.CloseAsync();

            return persistedJobs;
           
        }
        [HttpGet("{controller}/jobs/{id}")]
        public async Task<Job> GetJob(string id)
        {
            var persistedJob = await _jobStorage.FetchJob(id);

            await _jobClient.OpenAsync();
            var job = await _jobClient.GetJobAsync(persistedJob.Id);
            await _jobClient.CloseAsync();

            return persistedJob;
        }

        [HttpPost]
        public async Task<string> CreateJob(CreateJobRequest createJobRequest)
        {
            await _jobClient.OpenAsync();

            CloudToDeviceMethod directMethod = new CloudToDeviceMethod("", TimeSpan.FromSeconds(5),
      TimeSpan.FromSeconds(5));

            throw new NotImplementedException();
        }
    }
}
