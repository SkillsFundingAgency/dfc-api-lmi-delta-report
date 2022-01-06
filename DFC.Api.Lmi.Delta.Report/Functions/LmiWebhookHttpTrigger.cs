using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Enums;
using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Functions
{
    public class LmiWebhookHttpTrigger
    {
        private readonly ILogger<LmiWebhookHttpTrigger> logger;
        private readonly ILmiWebhookReceiverService lmiWebhookReceiverService;

        public LmiWebhookHttpTrigger(
           ILogger<LmiWebhookHttpTrigger> logger,
           ILmiWebhookReceiverService lmiWebhookReceiverService)
        {
            this.logger = logger;
            this.lmiWebhookReceiverService = lmiWebhookReceiverService;

            //TODO: ian: need to initialize the telemetry properly
            Activity? activity = null;
            if (Activity.Current == null)
            {
                activity = new Activity(nameof(LmiWebhookHttpTrigger)).Start();
                activity.SetParentId(Guid.NewGuid().ToString());
            }
        }

        [FunctionName("LmiWebhook")]
        [Display(Name = "LMI Webhook", Description = "Receives webhook Post requests for LMI delta report.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Request processed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data or wrong environment", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "lmi/webhook")] HttpRequest? request)
        {
            try
            {
                logger.LogInformation("Received webhook request");

                using var streamReader = new StreamReader(request?.Body!);
                var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(requestBody))
                {
                    logger.LogError($"{nameof(request)} body is null");
                    return new BadRequestResult();
                }

                var webhookRequestModel = lmiWebhookReceiverService.ExtractEvent(requestBody);
                switch (webhookRequestModel.WebhookCommand)
                {
                    case WebhookCommand.SubscriptionValidation:
                        return new OkObjectResult(webhookRequestModel.SubscriptionValidationResponse);
                    case WebhookCommand.ReportDeltaForAll:
                        return new StatusCodeResult((int)await lmiWebhookReceiverService.ReportAll().ConfigureAwait(false));
                    case WebhookCommand.ReportDeltaForSoc:
                        return new StatusCodeResult((int)await lmiWebhookReceiverService.ReportSoc(webhookRequestModel.ContentId).ConfigureAwait(false));
                    default:
                        return new BadRequestResult();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
