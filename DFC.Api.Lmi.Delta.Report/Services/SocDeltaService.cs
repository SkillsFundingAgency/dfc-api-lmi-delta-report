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

        public void DetermineDelta(DeltaReportModel? deltaReportModel)
        {
            _ = deltaReportModel ?? throw new ArgumentNullException(nameof(deltaReportModel));

            logger.LogInformation("Identifying delta for report");

            if (deltaReportModel.DeltaReportSocs != null && deltaReportModel.DeltaReportSocs.Any())
            {
                var jdp = new JsonDiffPatch();

                foreach (var deltaReportSoc in deltaReportModel.DeltaReportSocs)
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

            deltaReportModel.SocDeltaCount = (from a in deltaReportModel.DeltaReportSocs where !string.IsNullOrWhiteSpace(a.Delta) select a.Delta).Count();

            logger.LogInformation($"Identified {deltaReportModel.SocDeltaCount} deltas for report");
        }
    }
}
