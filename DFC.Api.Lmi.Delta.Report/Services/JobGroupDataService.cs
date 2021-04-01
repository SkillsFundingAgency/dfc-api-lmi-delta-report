using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Services
{
    public class JobGroupDataService : IJobGroupDataService
    {
        private readonly ILogger<JobGroupDataService> logger;
        private readonly IDraftJobGroupApiConnector draftJobGroupApiConnector;
        private readonly IPublishedJobGroupApiConnector publishedJobGroupApiConnector;

        public JobGroupDataService(
            ILogger<JobGroupDataService> logger,
            IDraftJobGroupApiConnector draftJobGroupApiConnector,
            IPublishedJobGroupApiConnector publishedJobGroupApiConnector)
        {
            this.logger = logger;
            this.draftJobGroupApiConnector = draftJobGroupApiConnector;
            this.publishedJobGroupApiConnector = publishedJobGroupApiConnector;
        }

        public async Task<DeltaReportModel> GetAllAsync()
        {
            logger.LogInformation("Loading SOC delta");

            var deltaReportModel = new DeltaReportModel { Id = Guid.NewGuid() };
            var draftSummaries = await draftJobGroupApiConnector.GetSummaryAsync().ConfigureAwait(false) ?? new List<JobGroupSummaryItemModel>();
            var publishedSummaries = await publishedJobGroupApiConnector.GetSummaryAsync().ConfigureAwait(false) ?? new List<JobGroupSummaryItemModel>();
            var draftSocs = (from a in draftSummaries select a.Soc).ToList();
            var publishedSocs = (from a in publishedSummaries select a.Soc).ToList();
            var allSocs = draftSocs.Union(publishedSocs).Distinct().ToList();

            deltaReportModel.DeltaReportSocs = (from soc in allSocs select new DeltaReportSocModel { Soc = soc, }).ToList();

            foreach (var deltaReportSocModel in deltaReportModel.DeltaReportSocs)
            {
                deltaReportSocModel.DraftJobGroup = await draftJobGroupApiConnector.GetDetailAsync(deltaReportSocModel.Soc).ConfigureAwait(false);
                deltaReportSocModel.PublishedJobGroup = await publishedJobGroupApiConnector.GetDetailAsync(deltaReportSocModel.Soc).ConfigureAwait(false);
            }

            return deltaReportModel;
        }

        public async Task<DeltaReportModel?> GetSocAsync(Guid? socId)
        {
            _ = socId ?? throw new ArgumentNullException(nameof(socId));

            logger.LogInformation($"Loading individual SOC data for: {socId}");

            var deltaReportSocModel = new DeltaReportSocModel
            {
                DraftJobGroup = await draftJobGroupApiConnector.GetDetailAsync(socId.Value).ConfigureAwait(false),
            };

            if (deltaReportSocModel.DraftJobGroup == null)
            {
                logger.LogInformation($"Failed to load individual draft SOC for: {socId}");
                return null;
            }

            deltaReportSocModel.PublishedJobGroup = await publishedJobGroupApiConnector.GetDetailAsync(deltaReportSocModel.DraftJobGroup.Soc).ConfigureAwait(false);
            deltaReportSocModel.Soc = deltaReportSocModel.DraftJobGroup.Soc;

            var deltaReportModel = new DeltaReportModel
            {
                Id = Guid.NewGuid(),
                DeltaReportSocs = new List<DeltaReportSocModel> { deltaReportSocModel },
            };

            return deltaReportModel;
        }
    }
}
