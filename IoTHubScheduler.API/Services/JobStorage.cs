using IoTHubScheduler.API.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTHubScheduler.API.Services
{
    public class JobStorage : IJobStorage
    {
        private CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;
        private readonly ILogger<JobStorage> _logger;
        private readonly string _databaseName;
        private readonly string _containerName;

        public JobStorage(ILogger<JobStorage> logger, IConfiguration configuration)
        {
            _logger = logger;
            _cosmosClient = new CosmosClient(configuration.GetValue<string>("CosmosEndpointUrl"), configuration.GetValue<string>("CosmosAuthToken"));
            _databaseName = configuration.GetValue<string>("CosmosDatabaseName");
            _containerName = configuration.GetValue<string>("CosmosContainerName");
        }
     
        public async Task<List<Job>> FetchAllJobs()
        {
            await PrepareCosmosData();

            var sqlQueryText = "SELECT * FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Job> queryResultSetIterator = _container.GetItemQueryIterator<Job>(queryDefinition);

            List<Job> jobs = new List<Job>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Job> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Job iotjob in currentResultSet)
                {
                    jobs.Add(iotjob);
                }
            }

            return jobs;
        }

        public async Task<Job> FetchJob(string id)
        {
            await PrepareCosmosData();

            var sqlQueryText = $"SELECT * FROM c WHERE c.ID = '{id}'";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Job> queryResultSetIterator = _container.GetItemQueryIterator<Job>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Job> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Job iotjob in currentResultSet)
                {
                    return iotjob;
                }
            }

            return null;
        }

        private async Task PrepareCosmosData()
        {
            _database = (await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName)).Database;
            // todo change partion key to type
            _container = (await _database.CreateContainerIfNotExistsAsync(_containerName, "/id")).Container;
        }

      
    }
}
