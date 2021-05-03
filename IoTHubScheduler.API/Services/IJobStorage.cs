using IoTHubScheduler.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IoTHubScheduler.API.Services
{
    public interface IJobStorage
    {
        Task<List<Job>> FetchAllJobs();
        Task<Job> FetchJob(string id);
        Task StoreJob(Job job);
        Task DeleteJob(string id);

    }
}