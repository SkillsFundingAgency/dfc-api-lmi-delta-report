using DFC.Api.Lmi.Delta.Report.Common;
using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Enums;
using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Api.Lmi.Delta.Report.Models.ClientOptions;
using DFC.Api.Lmi.Delta.Report.Models.FunctionRequestModels;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using DFC.Compui.Cosmos.Contracts;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Services
{
    public class LmiWebhookReceiverService : ILmiWebhookReceiverService
    {
        private const string EventTypePublished = "published";

        private readonly Dictionary<string, WebhookCacheOperation> acceptedEventTypes = new Dictionary<string, WebhookCacheOperation>
        {
            { "draft", WebhookCacheOperation.CreateOrUpdate },
            { EventTypePublished, WebhookCacheOperation.CreateOrUpdate },
            { "draft-discarded", WebhookCacheOperation.Delete },
            { "unpublished", WebhookCacheOperation.Delete },
            { "deleted", WebhookCacheOperation.Delete },
        };

        private readonly ILogger<LmiWebhookReceiverService> logger;
        private readonly IDocumentService<DeltaReportModel> deltaReportDocumentService;
        private readonly IJobGroupDataService jobGroupDataService;
        private readonly ISocDeltaService socDeltaService;
        private readonly IEventGridService eventGridService;
        private readonly PublishedJobGroupClientOptions publishedJobGroupClientOptions;
        private readonly EventGridClientOptions eventGridClientOptions;

        public LmiWebhookReceiverService(
            ILogger<LmiWebhookReceiverService> logger,
            IDocumentService<DeltaReportModel> deltaReportDocumentService,
            IJobGroupDataService jobGroupDataService,
            ISocDeltaService socDeltaService,
            IEventGridService eventGridService,
            PublishedJobGroupClientOptions publishedJobGroupClientOptions,
            EventGridClientOptions eventGridClientOptions)
        {
            this.logger = logger;
            this.deltaReportDocumentService = deltaReportDocumentService;
            this.jobGroupDataService = jobGroupDataService;
            this.socDeltaService = socDeltaService;
            this.eventGridService = eventGridService;
            this.publishedJobGroupClientOptions = publishedJobGroupClientOptions;
            this.eventGridClientOptions = eventGridClientOptions;
        }

        public static MessageContentType DetermineMessageContentType(string? apiEndpoint)
        {
            if (!string.IsNullOrWhiteSpace(apiEndpoint))
            {
                if (apiEndpoint.EndsWith($"/{Constants.ApiForJobGroups}", StringComparison.OrdinalIgnoreCase))
                {
                    return MessageContentType.JobGroup;
                }

                if (apiEndpoint.Contains($"/{Constants.ApiForJobGroups}/", StringComparison.OrdinalIgnoreCase))
                {
                    return MessageContentType.JobGroupItem;
                }
            }

            return MessageContentType.None;
        }

        public static WebhookCommand DetermineWebhookCommand(MessageContentType messageContentType, WebhookCacheOperation webhookCacheOperation)
        {
            switch (webhookCacheOperation)
            {
                case WebhookCacheOperation.CreateOrUpdate:
                    switch (messageContentType)
                    {
                        case MessageContentType.JobGroup:
                            return WebhookCommand.ReportDeltaForAll;
                        case MessageContentType.JobGroupItem:
                            return WebhookCommand.ReportDeltaForSoc;
                    }

                    break;
            }

            return WebhookCommand.None;
        }

        public WebhookRequestModel ExtractEvent(string requestBody)
        {
            var webhookRequestModel = new WebhookRequestModel();

            logger.LogInformation($"Received events: {requestBody}");

            var eventGridSubscriber = new EventGridSubscriber();
            foreach (var key in acceptedEventTypes.Keys)
            {
                eventGridSubscriber.AddOrUpdateCustomEventMapping(key, typeof(EventGridEventData));
            }

            var eventGridEvents = eventGridSubscriber.DeserializeEventGridEvents(requestBody);

            foreach (var eventGridEvent in eventGridEvents)
            {
                if (!Guid.TryParse(eventGridEvent.Id, out Guid eventId))
                {
                    throw new InvalidDataException($"Invalid Guid for EventGridEvent.Id '{eventGridEvent.Id}'");
                }

                if (eventGridEvent.Data is SubscriptionValidationEventData subscriptionValidationEventData)
                {
                    logger.LogInformation($"Got SubscriptionValidation event data, validationCode: {subscriptionValidationEventData!.ValidationCode},  validationUrl: {subscriptionValidationEventData.ValidationUrl}, topic: {eventGridEvent.Topic}");

                    webhookRequestModel.WebhookCommand = WebhookCommand.SubscriptionValidation;
                    webhookRequestModel.SubscriptionValidationResponse = new SubscriptionValidationResponse()
                    {
                        ValidationResponse = subscriptionValidationEventData.ValidationCode,
                    };

                    return webhookRequestModel;
                }
                else if (eventGridEvent.Data is EventGridEventData eventGridEventData)
                {
                    if (!Guid.TryParse(eventGridEventData.ItemId, out Guid contentId))
                    {
                        throw new InvalidDataException($"Invalid Guid for EventGridEvent.Data.ItemId '{eventGridEventData.ItemId}'");
                    }

                    if (!Uri.TryCreate(eventGridEventData.Api, UriKind.Absolute, out Uri? url))
                    {
                        throw new InvalidDataException($"Invalid Api url '{eventGridEventData.Api}' received for Event Id: {eventId}");
                    }

                    var cacheOperation = acceptedEventTypes[eventGridEvent.EventType];

                    logger.LogInformation($"Got Event Id: {eventId}: {eventGridEvent.EventType}: Cache operation: {cacheOperation} {url}");

                    var messageContentType = DetermineMessageContentType(url.ToString());
                    if (messageContentType == MessageContentType.None)
                    {
                        logger.LogError($"Event Id: {eventId} got unknown message content type - {messageContentType} - {url}");
                        return webhookRequestModel;
                    }

                    webhookRequestModel.WebhookCommand = DetermineWebhookCommand(messageContentType, cacheOperation);
                    webhookRequestModel.EventId = eventId;
                    webhookRequestModel.EventType = eventGridEvent.EventType;
                    webhookRequestModel.ContentId = contentId;
                    webhookRequestModel.Url = url;
                }
                else
                {
                    throw new InvalidDataException($"Invalid event type '{eventGridEvent.EventType}' received for Event Id: {eventId}, should be one of '{string.Join(",", acceptedEventTypes.Keys)}'");
                }
            }

            return webhookRequestModel;
        }

        public async Task<HttpStatusCode> ReportAll()
        {
            var deltaReportModel = await jobGroupDataService.GetAllAsync().ConfigureAwait(false);

            socDeltaService.DetermineDelta(deltaReportModel);

            logger.LogInformation("Saving SOC delta report");

            var result = await deltaReportDocumentService.UpsertAsync(deltaReportModel).ConfigureAwait(false);

            if (result == HttpStatusCode.Created)
            {
                await PostPublishedEventAsync($"Publish all SOCs to job-group app", eventGridClientOptions.ApiEndpoint).ConfigureAwait(false);
                await PurgeOldReportsAsync().ConfigureAwait(false);
            }

            return result;
        }

        public async Task<HttpStatusCode> ReportSoc(Guid? socId)
        {
            _ = socId ?? throw new ArgumentNullException(nameof(socId));

            var deltaReportModel = await jobGroupDataService.GetSocAsync(socId).ConfigureAwait(false);

            if (deltaReportModel == null)
            {
                return HttpStatusCode.NotFound;
            }

            socDeltaService.DetermineDelta(deltaReportModel);

            logger.LogInformation($"Saving individual SOC delta report for: {socId}");

            var result = await deltaReportDocumentService.UpsertAsync(deltaReportModel).ConfigureAwait(false);

            if (result == HttpStatusCode.Created)
            {
                var apiEndpoint = new Uri($"{eventGridClientOptions.ApiEndpoint}/{socId}", UriKind.Absolute);
                await PostPublishedEventAsync($"Publish individual SOC {socId} to job-group app", apiEndpoint).ConfigureAwait(false);
                await PurgeOldReportsAsync().ConfigureAwait(false);
            }

            return result;
        }

        public async Task PurgeOldReportsAsync()
        {
            logger.LogInformation("Purging old delta reports");

            var allDeltaReports = await deltaReportDocumentService.GetAllAsync().ConfigureAwait(false);
            var purgeDeltaReports = allDeltaReports.OrderByDescending(o => o.CreatedDate).Skip(publishedJobGroupClientOptions.MaxReportsKept);

            foreach (var deltaReport in purgeDeltaReports)
            {
                logger.LogInformation($"Purging old delta report: {deltaReport.CreatedDate.ToString("O", CultureInfo.InvariantCulture)} - {deltaReport.Id}");
                await deltaReportDocumentService.DeleteAsync(deltaReport.Id).ConfigureAwait(false);
            }
        }

        public async Task PostPublishedEventAsync(string displayText, Uri? apiEndpoint)
        {
            logger.LogInformation($"Posting to event grid for: {displayText}");

            var eventGridEventData = new EventGridEventData
            {
                ItemId = Guid.NewGuid().ToString(),
                Api = apiEndpoint?.ToString(),
                DisplayText = displayText,
                VersionId = Guid.NewGuid().ToString(),
                Author = eventGridClientOptions.SubjectPrefix,
            };

            await eventGridService.SendEventAsync(eventGridEventData, eventGridClientOptions.SubjectPrefix, EventTypePublished).ConfigureAwait(false);
        }
    }
}
