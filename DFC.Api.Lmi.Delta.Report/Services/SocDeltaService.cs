﻿using AutoMapper;
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

            fullDeltaReportModel.SocImportedCount = fullDeltaReportModel.DeltaReportSocs!.Count;
            fullDeltaReportModel.SocAdditionCount = (from a in fullDeltaReportModel.DeltaReportSocs where a.DraftJobGroup != null && a.PublishedJobGroup == null select a).Count();
            fullDeltaReportModel.SocUpdateCount = (from a in fullDeltaReportModel.DeltaReportSocs where a.DraftJobGroup != null && a.PublishedJobGroup != null && !string.IsNullOrWhiteSpace(a.Delta) select a).Count();
            fullDeltaReportModel.SocDeletionCount = (from a in fullDeltaReportModel.DeltaReportSocs where a.DraftJobGroup == null && a.PublishedJobGroup != null select a).Count();

            logger.LogInformation($"Imported {fullDeltaReportModel.SocImportedCount} SOCs for report");
            logger.LogInformation($"Identified {fullDeltaReportModel.SocAdditionCount} additions for report");
            logger.LogInformation($"Identified {fullDeltaReportModel.SocUpdateCount} updates for report");
            logger.LogInformation($"Identified {fullDeltaReportModel.SocDeletionCount} deletions for report");
        }
    }
}
