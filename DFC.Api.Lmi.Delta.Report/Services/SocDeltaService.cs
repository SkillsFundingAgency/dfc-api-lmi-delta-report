using AutoMapper;
using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using JsonDiffPatchDotNet;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace DFC.Api.Lmi.Delta.Report.Services
{
    public class SocDeltaService : ISocDeltaService
    {
        private readonly ILogger<SocDeltaService> logger;
        private readonly IMapper mapper;

        public SocDeltaService(ILogger<SocDeltaService> logger, IMapper mapper)
        {
            this.logger = logger;
            this.mapper = mapper;
        }

        public void DetermineDelta(FullDeltaReportModel? fullDeltaReportModel)
        {
            _ = fullDeltaReportModel ?? throw new ArgumentNullException(nameof(fullDeltaReportModel));

            logger.LogInformation("Identifying delta for report");

            if (fullDeltaReportModel.DeltaReportSocs != null && fullDeltaReportModel.DeltaReportSocs.Any())
            {
                var jdp = new JsonDiffPatch();

                foreach (var deltaReportSoc in fullDeltaReportModel.DeltaReportSocs)
                {
                    var publishedJobGroupToDelta = mapper.Map<JobGroupToDeltaModel>(deltaReportSoc.PublishedJobGroup);
                    var draftJobGroupToDelta = mapper.Map<JobGroupToDeltaModel>(deltaReportSoc.DraftJobGroup);
                    var published = JToken.Parse(JsonConvert.SerializeObject(publishedJobGroupToDelta));
                    var draft = JToken.Parse(JsonConvert.SerializeObject(draftJobGroupToDelta));
                    var delta = jdp.Diff(published, draft);

                    if (delta != null)
                    {
                        deltaReportSoc.Delta = delta.ToString();
                    }
                }
            }

            fullDeltaReportModel.SocDeltaCount = (from a in fullDeltaReportModel.DeltaReportSocs where !string.IsNullOrWhiteSpace(a.Delta) select a.Delta).Count();

            logger.LogInformation($"Identified {fullDeltaReportModel.SocDeltaCount} deltas for report");
        }
    }
}
