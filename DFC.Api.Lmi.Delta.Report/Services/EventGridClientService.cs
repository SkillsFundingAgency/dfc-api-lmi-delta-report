﻿using DFC.Api.Lmi.Delta.Report.Contracts;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Services
{
    [ExcludeFromCodeCoverage]
    public class EventGridClientService : IEventGridClientService
    {
        private readonly ILogger<EventGridClientService> logger;

        public EventGridClientService(ILogger<EventGridClientService> logger)
        {
            this.logger = logger;
        }

        public async Task SendEventAsync(List<EventGridEvent>? eventGridEvents, string? topicEndpoint, string? topicKey, string? logMessage)
        {
            _ = eventGridEvents ?? throw new ArgumentNullException(nameof(eventGridEvents));
            _ = topicEndpoint ?? throw new ArgumentNullException(nameof(topicEndpoint));
            _ = topicKey ?? throw new ArgumentNullException(nameof(topicKey));
            _ = logMessage ?? throw new ArgumentNullException(nameof(logMessage));

            logger.LogInformation($"Sending Event Grid message for: {logMessage}");

            try
            {
                string topicHostname = new Uri(topicEndpoint).Host;
                var topicCredentials = new TopicCredentials(topicKey);
                using var client = new EventGridClient(topicCredentials);

                await client.PublishEventsAsync(topicHostname, eventGridEvents).ConfigureAwait(false);

                logger.LogInformation($"Sent Event Grid message for: {logMessage}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception sending Event Grid message for: {logMessage}");
            }
        }
    }
}
