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
        [HttpGet("jobs/{id}")]
        public async Task<Job> GetJob(string id)
        {
            var persistedJob = await _jobStorage.FetchJob(id);

            await _jobClient.OpenAsync();
            var job = await _jobClient.GetJobAsync(id);
            await _jobClient.CloseAsync();

            return persistedJob;
        }

        [HttpPost]
        public async Task<string> CreateJob(CreateJobRequest createJobRequest)
        {
            var jobId = Guid.NewGuid().ToString();

            await _jobClient.OpenAsync();

            if(createJobRequest.Type == Models.JobType.DirectMethod)
            {
                CloudToDeviceMethod directMethod = new CloudToDeviceMethod("HandleDirectMethod", TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(5));

                directMethod.SetPayloadJson(createJobRequest.Data);

                await _jobClient.ScheduleDeviceMethodAsync(jobId, createJobRequest.Query, directMethod, DateTime.UtcNow, 5);
            }

            await _jobClient.CloseAsync();

           // var job = await _jobClient.GetJobAsync(jobId);

            return await Task.FromResult(jobId);
        }
    }
}
