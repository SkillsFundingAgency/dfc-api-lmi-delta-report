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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Functions
{
    public class GetDeltaReportsHttpTrigger
    {
        private readonly ILogger<GetDeltaReportsHttpTrigger> logger;
        private readonly IMapper mapper;
        private readonly IDocumentService<DeltaReportModel> deltaReportDocumentService;

        public GetDeltaReportsHttpTrigger(
           ILogger<GetDeltaReportsHttpTrigger> logger,
           IMapper mapper,
           IDocumentService<DeltaReportModel> deltaReportDocumentService)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.deltaReportDocumentService = deltaReportDocumentService;
        }

        [FunctionName("DeltaReports")]
        [Display(Name = "Get all delta reports", Description = "Retrieves all of the delta reports.")]
        [ProducesResponseType(typeof(DeltaReportSummaryApiModel), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data or wrong environment", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "delta-reports")] HttpRequest? request)
        {
            logger.LogInformation("Getting all delta reports");

            var deltaReportModels = await deltaReportDocumentService.GetAllAsync().ConfigureAwait(false);

            if (deltaReportModels != null && deltaReportModels.Any())
            {
                logger.LogInformation($"Returning {deltaReportModels.Count()} delta reports");

                var results = mapper.Map<IList<DeltaReportSummaryApiModel>>(deltaReportModels);

                return new OkObjectResult(results.OrderByDescending(o => o.CreatedDate));
            }

            logger.LogWarning("Failed to get any delta reports");

            return new NoContentResult();
        }
    }
}
