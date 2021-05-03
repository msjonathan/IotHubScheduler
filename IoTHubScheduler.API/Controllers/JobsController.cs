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

        public JobsController(ILogger<JobsController> logger, IConfiguration configuration, IJobStorage jobStorage)
        {
            _logger = logger;
            _jobClient = JobClient.CreateFromConnectionString(configuration.GetConnectionString("iothub"));
            _jobStorage = jobStorage;
        }

        [HttpGet]
        public async Task<IEnumerable<JobResponse>> GetJobs(string status)
        {
            var retVal = new List<JobResponse>();
            // with the CreateQuery we can consult the 
            // open questions how long are the jobs available as history?
            var jobs = _jobClient.CreateQuery();

            while(jobs.HasMoreResults)
            {
               var jobResponses = await jobs.GetNextAsJobResponseAsync();

                foreach (var jobResponse in jobResponses)
                {
                    retVal.Add(jobResponse);
                }
            }

            await _jobClient.CloseAsync();

            if(!string.IsNullOrEmpty(status))
            {
                return retVal.Where(f => f.Status.ToString() == status);
            }
            return retVal;

        }

        [HttpGet("{id}")]
        public async Task<JobResponse> GetJob(string id)
        {
            await _jobClient.OpenAsync();
            var job = await _jobClient.GetJobAsync(id);
            await _jobClient.CloseAsync();

            return job;
        }


        [HttpPost]
        public async Task<string> CreateJob(CreateJobRequest createJobRequest)
        {
            try
            {
                var jobId = Guid.NewGuid().ToString();

                await _jobClient.OpenAsync();

                if (createJobRequest.Type == Models.JobType.DirectMethod)
                {
                    CloudToDeviceMethod directMethod = new CloudToDeviceMethod("HandleDirectMethod", TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(5));

                    directMethod.SetPayloadJson(createJobRequest.Data);

                    await _jobClient.ScheduleDeviceMethodAsync(jobId, createJobRequest.Query, directMethod, DateTime.UtcNow, 5);
                }
                if(createJobRequest.Type == Models.JobType.UpdateDeviceTwin)
                {
                    var twin = new Twin();

                    var twinChanges = JObject.Parse(createJobRequest.Data);

                    foreach (JProperty tag in (JToken)twinChanges["Tags"])
                    {
                        twin.Tags[tag.Name] = (string)tag.Value;
                    }
                    foreach (JProperty property in (JToken)twinChanges["DesiredProperties"])
                    {
                        // this does not yet support a complex json structure
                        twin.Properties.Desired[property.Name] = (string)property.Value;
                    }
                    
                    await _jobClient.ScheduleTwinUpdateAsync(jobId, createJobRequest.Query, twin, DateTime.UtcNow, 5);                
                }

                await _jobClient.CloseAsync();

                // not really neccecary but just for poc purposes we also store it in a redis store
                await _jobStorage.StoreJob(new Job()
                {
                    Type = createJobRequest.Type.ToString(),
                    CreationTime = DateTime.UtcNow,
                    Id = jobId,
                    Data = createJobRequest.Query
                });

                return await Task.FromResult(jobId);
            }
            catch (Exception)
            {
                // this is a poc, we are not interested in the scenario's where it goes wrong
                throw;
            }


        }
    }
}
