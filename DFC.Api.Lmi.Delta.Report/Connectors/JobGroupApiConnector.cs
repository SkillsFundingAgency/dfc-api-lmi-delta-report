using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Api.Lmi.Delta.Report.Models.ClientOptions;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Connectors
{
    public abstract class JobGroupApiConnector : IJobGroupApiConnector
    {
        private readonly ILogger<JobGroupApiConnector> logger;
        private readonly HttpClient httpClient;
        private readonly IApiDataConnector apiDataConnector;
        private readonly JobGroupClientOptions jobGroupClientOptions;

        public JobGroupApiConnector(
            ILogger<JobGroupApiConnector> logger,
            HttpClient httpClient,
            IApiDataConnector apiDataConnector,
            JobGroupClientOptions jobGroupClientOptions)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.apiDataConnector = apiDataConnector;
            this.jobGroupClientOptions = jobGroupClientOptions;
        }

        public async Task<IList<JobGroupSummaryItemModel>?> GetSummaryAsync()
        {
            var url = new Uri($"{jobGroupClientOptions.BaseAddress}{jobGroupClientOptions.SummaryEndpoint}");
            logger.LogInformation($"Retrieving summaries from job-groups app: {url}");
            return await apiDataConnector.GetAsync<List<JobGroupSummaryItemModel>>(httpClient, url).ConfigureAwait(false);
        }

        public async Task<JobGroupModel?> GetDetailAsync(int soc)
        {
            var url = new Uri($"{jobGroupClientOptions.BaseAddress}{jobGroupClientOptions.DetailEndpoint}/soc/{soc}");
            logger.LogInformation($"Retrieving details from job-groups app: {url}");
            return await apiDataConnector.GetAsync<JobGroupModel>(httpClient, url).ConfigureAwait(false);
        }

        public async Task<JobGroupModel?> GetDetailAsync(Guid socId)
        {
            var url = new Uri($"{jobGroupClientOptions.BaseAddress}{jobGroupClientOptions.DetailEndpoint}/{socId}");
            logger.LogInformation($"Retrieving details from job-groups app: {url}");
            return await apiDataConnector.GetAsync<JobGroupModel>(httpClient, url).ConfigureAwait(false);
        }
    }
}
