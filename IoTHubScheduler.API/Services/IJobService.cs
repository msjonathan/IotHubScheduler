using IoTHubScheduler.API.Models;
using Microsoft.Azure.Devices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IoTHubScheduler.API.Services
{
    public interface IJobService
    {
        Task<List<JobResponse>> GetAllJobs();
        Task<List<JobResponse>> GetAllJobsByStatus(JobStatus status);
        Task<JobResponse> GetJob(string id);
        Task<string> CreateJob(CreateJobRequest createJobRequest);
    }
}