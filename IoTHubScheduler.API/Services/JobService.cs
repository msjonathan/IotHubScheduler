using IoTHubScheduler.API.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IoTHubScheduler.API.Services
{
    public class JobService : IJobService, IDisposable
    {
        private readonly ILogger<JobService> _logger;
        private JobClient _jobClient;
        private IJobStorage _jobStorage;

        public JobService(ILogger<JobService> logger, IConfiguration configuration, IJobStorage jobStorage)
        {
            _logger = logger;
            _jobClient = JobClient.CreateFromConnectionString(configuration.GetConnectionString("iothub"));
            _jobStorage = jobStorage;

            // this need to move in production code - Open-Close principle
            _jobClient.OpenAsync();
        }

        public async Task<string> CreateJob(CreateJobRequest createJobRequest)
        {
            // first lookup if there is any running job 
            var runningJobs = await GetAllJobsByStatus(JobStatus.Running);


            var startTime = runningJobs.Count() > 0 ? runningJobs.First().StartTimeUtc.Value.AddSeconds(runningJobs.First().MaxExecutionTimeInSeconds).AddSeconds(10) : DateTime.UtcNow;

            var jobId = Guid.NewGuid().ToString();

            if (createJobRequest.Type == Models.JobType.DirectMethod)
            {
                CloudToDeviceMethod directMethod = new CloudToDeviceMethod("HandleDirectMethod", TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(5));

                directMethod.SetPayloadJson(createJobRequest.Data);

                await _jobClient.ScheduleDeviceMethodAsync(jobId, createJobRequest.Query, directMethod, startTime, 5);
            }
            if (createJobRequest.Type == Models.JobType.UpdateDeviceTwin)
            {
                var twin = new Twin();

                var twinChanges = JObject.Parse(createJobRequest.Data);

                if (twinChanges["Tags"] != null)
                {
                    foreach (JProperty tag in (JToken)twinChanges["Tags"])
                    {
                        twin.Tags[tag.Name] = (string)tag.Value;
                    }
                }

                if (twinChanges["DesiredProperties"] != null)
                {
                    foreach (JProperty property in (JToken)twinChanges["DesiredProperties"])
                    {
                        // this does not yet support a complex json structure
                        twin.Properties.Desired[property.Name] = (string)property.Value;
                    }
                }

                await _jobClient.ScheduleTwinUpdateAsync(jobId, createJobRequest.Query, twin, startTime, maxExecutionTimeInSeconds: 15);
            }

            await _jobStorage.StoreJob(new Job()
            {
                Type = createJobRequest.Type.ToString(),
                CreationTime = DateTime.UtcNow,
                Id = jobId,
                Data = createJobRequest.Query
            });

            return jobId;
        }

        public void Dispose()
        {
            if (_jobClient != null)
            {
                _jobClient.CloseAsync();
            }
        }

        public async Task<List<JobResponse>> GetAllJobs()
        {
            var retVal = new List<JobResponse>();

            var jobs = _jobClient.CreateQuery();

            while (jobs.HasMoreResults)
            {
                var jobResponses = await jobs.GetNextAsJobResponseAsync();

                foreach (var jobResponse in jobResponses)
                {
                    retVal.Add(jobResponse);
                }
            }

            return retVal;
        }

        public async Task<List<JobResponse>> GetAllJobsByStatus(JobStatus status)
        {
            return (await GetAllJobs()).Where(f => f.Status == status).ToList();
        }

        public async Task<JobResponse> GetJob(string id)
        {
            return await _jobClient.GetJobAsync(id);
        }
    }
}
