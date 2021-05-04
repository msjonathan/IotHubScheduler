using IoTHubScheduler.API.Models;
using IoTHubScheduler.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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
        private IJobService _jobService;

        public JobsController(ILogger<JobsController> logger, IConfiguration configuration, IJobStorage jobStorage, IJobService jobService)
        {
            _logger = logger;
            _jobClient = JobClient.CreateFromConnectionString(configuration.GetConnectionString("iothub"));
            _jobStorage = jobStorage;
            _jobService = jobService;
        }

        [HttpGet]
        public async Task<IEnumerable<JobResponse>> GetJobs(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return await _jobService.GetAllJobs();
            }

            return await _jobService.GetAllJobsByStatus(Enum.Parse<JobStatus>(status));
        }

        [HttpGet("{id}")]
        public async Task<JobResponse> GetJob(string id)
        {
            return await _jobService.GetJob(id);
        }


        [HttpPost]
        public async Task<string> CreateJob(CreateJobRequest createJobRequest)
        {
           return await _jobService.CreateJob(createJobRequest);
        }
    }
}
