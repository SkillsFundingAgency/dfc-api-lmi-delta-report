using AutoMapper;
using DFC.Api.Lmi.Delta.Report.Models.ApiModels;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using DFC.Compui.Cosmos.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Functions
{
    public class GetDeltaReportHttpTrigger
    {
        private readonly ILogger<GetDeltaReportHttpTrigger> logger;
        private readonly IMapper mapper;
        private readonly IDocumentService<DeltaReportModel> deltaReportDocumentService;
        private readonly IDocumentService<DeltaReportSocModel> deltaReportSocDocumentService;

        public GetDeltaReportHttpTrigger(
           ILogger<GetDeltaReportHttpTrigger> logger,
           IMapper mapper,
           IDocumentService<DeltaReportModel> deltaReportDocumentService,
           IDocumentService<DeltaReportSocModel> deltaReportSocDocumentService)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.deltaReportDocumentService = deltaReportDocumentService;
            this.deltaReportSocDocumentService = deltaReportSocDocumentService;
        }

        [FunctionName("DeltaReport")]
        [Display(Name = "Get a delta report", Description = "Retrieve a delta report.")]
        [ProducesResponseType(typeof(DeltaReportApiModel), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data or wrong environment", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "delta-reports/{id}")] HttpRequest? request, Guid id)
        {
            logger.LogInformation($"Getting delta reports for id: {id}");

            var deltaReportModel = await deltaReportDocumentService.GetByIdAsync(id).ConfigureAwait(false);

            if (deltaReportModel != null)
            {
                logger.LogInformation($"Returning delta report for id: {id}");

                var result = mapper.Map<DeltaReportApiModel>(deltaReportModel);
                var deltaReportSocModels = await deltaReportSocDocumentService.GetAsync(w => w.DeltaReportId == id).ConfigureAwait(false);

                result.DeltaReportSocs = mapper.Map<List<DeltaReportSocApiModel>>(deltaReportSocModels);

                return new OkObjectResult(result);
            }

            logger.LogWarning("Failed to get delta report");

            return new NoContentResult();
        }
    }
}
