using DFC.Api.Lmi.Delta.Report.Enums;
using Microsoft.Azure.EventGrid.Models;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.FunctionRequestModels
{
    [ExcludeFromCodeCoverage]
    public class WebhookRequestModel
    {
        public WebhookCommand WebhookCommand { get; set; }

        public Guid? EventId { get; set; }

        public string? EventType { get; set; }

        public Guid? ContentId { get; set; }

        public Uri? Url { get; set; }

        public SubscriptionValidationResponse? SubscriptionValidationResponse { get; set; }
    }
}
