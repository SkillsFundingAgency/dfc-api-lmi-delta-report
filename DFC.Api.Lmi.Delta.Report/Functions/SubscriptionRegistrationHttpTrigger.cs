using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Compui.Subscriptions.Pkg.NetStandard.Data.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace DFC.Api.Lmi.Delta.Report.Functions
{
    public class SubscriptionRegistrationHttpTrigger
    {
        private readonly ILogger<SubscriptionRegistrationHttpTrigger> logger;
        private readonly EnvironmentValues environmentValues;
        private readonly ISubscriptionRegistrationService subscriptionRegistrationService;

        public SubscriptionRegistrationHttpTrigger(
           ILogger<SubscriptionRegistrationHttpTrigger> logger,
           EnvironmentValues environmentValues,
           ISubscriptionRegistrationService subscriptionRegistrationService)
        {
            this.logger = logger;
            this.environmentValues = environmentValues;
            this.subscriptionRegistrationService = subscriptionRegistrationService;
        }

        [FunctionName("SubscriptionRegistration")]
        [Display(Name = "Register Event Grid Subscription", Description = "Register an Event Grid Subscription to an Event Grid Topic")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Subscription Registration updated", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "SubscriptionRegistration error(s)", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscription/")] HttpRequest? request)
        {
            logger.LogInformation("Request received for SubscriptionRegistration");
            try
            {
                var apiSuffix = environmentValues.EnvironmentNameApiSuffix;
                if (!string.IsNullOrWhiteSpace(apiSuffix))
                {
                    apiSuffix = "-" + apiSuffix
                                .Replace("(", string.Empty, StringComparison.Ordinal)
                                .Replace(")", string.Empty, StringComparison.Ordinal)
                                .Replace(" ", "-", StringComparison.Ordinal).ToLowerInvariant();
                }

                await subscriptionRegistrationService.RegisterSubscription("DFC-Api-LMI-Delta-Reports" + apiSuffix).ConfigureAwait(false);
                return new OkResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error creating SubscriptionRegistration");
                return new InternalServerErrorResult();
            }
        }
    }
}
