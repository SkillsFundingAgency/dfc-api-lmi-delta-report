using DFC.Api.Lmi.Delta.Report.Common;
using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Enums;
using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Api.Lmi.Delta.Report.Models.ClientOptions;
using DFC.Api.Lmi.Delta.Report.Models.FunctionRequestModels;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using DFC.Api.Lmi.Delta.Report.Services;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.ServiceTests
{
    [Trait("Category", "LmiWebhookReceiverService - service Unit Tests")]
    public class LmiWebhookReceiverServiceTests
    {
        protected const string EventTypePublished = "published";
        protected const string EventTypeDeleted = "deleted";

        private readonly ILogger<LmiWebhookReceiverService> fakeLogger = A.Fake<ILogger<LmiWebhookReceiverService>>();
        private readonly IDocumentService<DeltaReportModel> fakeDeltaReportDocumentService = A.Fake<IDocumentService<DeltaReportModel>>();
        private readonly IJobGroupDataService fakeJobGroupDataService = A.Fake<IJobGroupDataService>();
        private readonly ISocDeltaService fakeSocDeltaService = A.Fake<ISocDeltaService>();
        private readonly IEventGridService fakeEventGridService = A.Fake<IEventGridService>();
        private readonly LmiWebhookReceiverService lmiWebhookReceiverService;
        private readonly PublishedJobGroupClientOptions publishedJobGroupClientOptions = new PublishedJobGroupClientOptions();
        private readonly EventGridClientOptions eventGridClientOptions = new EventGridClientOptions();

        public LmiWebhookReceiverServiceTests()
        {
            lmiWebhookReceiverService = new LmiWebhookReceiverService(fakeLogger, fakeDeltaReportDocumentService, fakeJobGroupDataService, fakeSocDeltaService, fakeEventGridService, publishedJobGroupClientOptions, eventGridClientOptions);
        }

        [Theory]
        [InlineData(null, MessageContentType.None)]
        [InlineData("", MessageContentType.None)]
        [InlineData("https://somewhere.com/api/" + Constants.ApiForJobGroups, MessageContentType.JobGroup)]
        [InlineData("https://somewhere.com/api/" + Constants.ApiForJobGroups + "/", MessageContentType.JobGroupItem)]
        public void LmiWebhookReceiverServiceDetermineMessageContentTypeReturnsExpected(string? apiEndpoint, MessageContentType expectedResult)
        {
            // Arrange

            // Act
            var result = LmiWebhookReceiverService.DetermineMessageContentType(apiEndpoint);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(MessageContentType.None, WebhookCacheOperation.None, WebhookCommand.None)]
        [InlineData(MessageContentType.None, WebhookCacheOperation.CreateOrUpdate, WebhookCommand.None)]
        [InlineData(MessageContentType.None, WebhookCacheOperation.Delete, WebhookCommand.None)]
        [InlineData(MessageContentType.JobGroup, WebhookCacheOperation.None, WebhookCommand.None)]
        [InlineData(MessageContentType.JobGroup, WebhookCacheOperation.CreateOrUpdate, WebhookCommand.ReportDeltaForAll)]
        [InlineData(MessageContentType.JobGroupItem, WebhookCacheOperation.None, WebhookCommand.None)]
        [InlineData(MessageContentType.JobGroupItem, WebhookCacheOperation.CreateOrUpdate, WebhookCommand.ReportDeltaForSoc)]
        public void LmiWebhookReceiverServiceDetermineWebhookCommandReturnsExpected(MessageContentType messageContentType, WebhookCacheOperation webhookCacheOperation, WebhookCommand expectedResult)
        {
            // Arrange

            // Act
            var result = LmiWebhookReceiverService.DetermineWebhookCommand(messageContentType, webhookCacheOperation);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void LmiWebhookReceiverServiceExtractEventReturnsExpectedSubscriptionRequest()
        {
            // Arrange
            var subscriptionValidationEventData = new SubscriptionValidationEventData("a validation code", "a validation url");
            var expectedResult = new WebhookRequestModel
            {
                WebhookCommand = WebhookCommand.SubscriptionValidation,
                SubscriptionValidationResponse = new SubscriptionValidationResponse { ValidationResponse = subscriptionValidationEventData.ValidationCode },
            };
            var eventGridEvents = BuildValidEventGridEvent(Microsoft.Azure.EventGrid.EventTypes.EventGridSubscriptionValidationEvent, subscriptionValidationEventData);
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var result = lmiWebhookReceiverService.ExtractEvent(requestBody);

            // Assert
            Assert.Equal(expectedResult.WebhookCommand, result.WebhookCommand);
            Assert.Equal(expectedResult.SubscriptionValidationResponse.ValidationResponse, result.SubscriptionValidationResponse?.ValidationResponse);
        }

        [Theory]
        [InlineData(EventTypePublished, WebhookCommand.ReportDeltaForAll, "https://somewhere.com/api/" + Constants.ApiForJobGroups)]
        [InlineData(EventTypePublished, WebhookCommand.ReportDeltaForSoc, "https://somewhere.com/api/" + Constants.ApiForJobGroups + "/")]
        public void LmiWebhookReceiverServiceExtractEventReturnsExpected(string eventType, WebhookCommand webhookCommand, string api)
        {
            // Arrange
            var eventGridEventData = new EventGridEventData
            {
                ItemId = Guid.NewGuid().ToString(),
                Api = api,
            };
            var eventGridEvents = BuildValidEventGridEvent(eventType, eventGridEventData);
            var expectedResult = new WebhookRequestModel
            {
                WebhookCommand = webhookCommand,
                EventId = Guid.Parse(eventGridEvents.First().Id),
                EventType = eventGridEvents.First().EventType,
                ContentId = Guid.Parse(eventGridEventData.ItemId),
                Url = new Uri(eventGridEventData.Api, UriKind.Absolute),
                SubscriptionValidationResponse = null,
            };
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var result = lmiWebhookReceiverService.ExtractEvent(requestBody);

            // Assert
            Assert.Equal(expectedResult.WebhookCommand, result.WebhookCommand);
            Assert.Equal(expectedResult.EventType, result.EventType);
            Assert.Equal(expectedResult.ContentId, result.ContentId);
            Assert.Equal(expectedResult.Url, result.Url);
            Assert.Null(result.SubscriptionValidationResponse?.ValidationResponse);
        }

        [Fact]
        public void LmiWebhookReceiverServiceExtractEventRaisesExceptionForInvalidEventId()
        {
            // Arrange
            var eventGridEventData = new EventGridEventData
            {
                ItemId = Guid.NewGuid().ToString(),
                Api = "https://somewhere.com",
            };
            var eventGridEvents = BuildValidEventGridEvent(EventTypePublished, eventGridEventData);
            eventGridEvents.First().Id = string.Empty;
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var exceptionResult = Assert.Throws<InvalidDataException>(() => lmiWebhookReceiverService.ExtractEvent(requestBody));

            // Assert
            Assert.Equal($"Invalid Guid for EventGridEvent.Id '{eventGridEvents.First().Id}'", exceptionResult.Message);
        }

        [Fact]
        public void LmiWebhookReceiverServiceExtractEventRaisesExceptionForInvalidItemId()
        {
            // Arrange
            var eventGridEventData = new EventGridEventData
            {
                ItemId = string.Empty,
                Api = "https://somewhere.com",
            };
            var eventGridEvents = BuildValidEventGridEvent(EventTypePublished, eventGridEventData);
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var exceptionResult = Assert.Throws<InvalidDataException>(() => lmiWebhookReceiverService.ExtractEvent(requestBody));

            // Assert
            Assert.Equal($"Invalid Guid for EventGridEvent.Data.ItemId '{eventGridEventData.ItemId}'", exceptionResult.Message);
        }

        [Fact]
        public void LmiWebhookReceiverServiceExtractEventRaisesExceptionForInvalidApi()
        {
            // Arrange
            var eventGridEventData = new EventGridEventData
            {
                ItemId = Guid.NewGuid().ToString(),
                Api = "https:somewhere.com",
            };
            var eventGridEvents = BuildValidEventGridEvent(EventTypePublished, eventGridEventData);
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var exceptionResult = Assert.Throws<InvalidDataException>(() => lmiWebhookReceiverService.ExtractEvent(requestBody));

            // Assert
            Assert.Equal($"Invalid Api url '{eventGridEventData.Api}' received for Event Id: {eventGridEvents.First().Id}", exceptionResult.Message);
        }

        [Fact]
        public void LmiWebhookReceiverServiceExtractEventReturnsNone()
        {
            // Arrange
            var eventGridEventData = new EventGridEventData
            {
                ItemId = Guid.NewGuid().ToString(),
                Api = "https://somewhere.com/api/",
            };
            var eventGridEvents = BuildValidEventGridEvent(EventTypePublished, eventGridEventData);
            var expectedResult = new WebhookRequestModel
            {
                WebhookCommand = WebhookCommand.None,
            };
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var result = lmiWebhookReceiverService.ExtractEvent(requestBody);

            // Assert
            Assert.Equal(expectedResult.WebhookCommand, result.WebhookCommand);
        }

        [Fact]
        public void LmiWebhookReceiverServiceExtractEventRaisesExceptionForEventData()
        {
            // Arrange
            EventGridEventData? nullEventGridEventData = null;
            var eventGridEvents = BuildValidEventGridEvent(EventTypePublished, nullEventGridEventData);
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var exceptionResult = Assert.Throws<InvalidDataException>(() => lmiWebhookReceiverService.ExtractEvent(requestBody));

            // Assert
            Assert.Equal($"Invalid event type '{eventGridEvents.First().EventType}' received for Event Id: {eventGridEvents.First().Id}, should be one of 'draft,published,draft-discarded,unpublished,deleted'", exceptionResult.Message);
        }

        [Fact]
        public async Task LmiWebhookReceiverServiceReportAllReturnsSuccessfully()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.Created;
            var dummyDeltaReportModel = A.Dummy<DeltaReportModel>();
            var dummyDeltaReports = A.CollectionOfDummy<DeltaReportModel>(publishedJobGroupClientOptions.MaxReportsKept + 1);

            A.CallTo(() => fakeJobGroupDataService.GetAllAsync()).Returns(dummyDeltaReportModel);
            A.CallTo(() => fakeDeltaReportDocumentService.UpsertAsync(A<DeltaReportModel>.Ignored)).Returns(HttpStatusCode.Created);
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).Returns(dummyDeltaReports);
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(true);

            // Act
            var result = await lmiWebhookReceiverService.ReportAll().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupDataService.GetAllAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeSocDeltaService.DetermineDelta(A<DeltaReportModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.UpsertAsync(A<DeltaReportModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappened(dummyDeltaReports.Count - publishedJobGroupClientOptions.MaxReportsKept, Times.Exactly);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiWebhookReceiverServiceReportSocReturnsSuccessfully()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.Created;
            var dummyDeltaReportModel = A.Dummy<DeltaReportModel>();
            var dummyDeltaReports = A.CollectionOfDummy<DeltaReportModel>(publishedJobGroupClientOptions.MaxReportsKept + 1);

            A.CallTo(() => fakeJobGroupDataService.GetSocAsync(A<Guid>.Ignored)).Returns(dummyDeltaReportModel);
            A.CallTo(() => fakeDeltaReportDocumentService.UpsertAsync(A<DeltaReportModel>.Ignored)).Returns(HttpStatusCode.Created);
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).Returns(dummyDeltaReports);
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(true);

            // Act
            var result = await lmiWebhookReceiverService.ReportSoc(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupDataService.GetSocAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeSocDeltaService.DetermineDelta(A<DeltaReportModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.UpsertAsync(A<DeltaReportModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappened(dummyDeltaReports.Count - publishedJobGroupClientOptions.MaxReportsKept, Times.Exactly);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiWebhookReceiverServiceReportSocThrowsExceoptionWhenNullSocId()
        {
            // Arrange

            // Act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await lmiWebhookReceiverService.ReportSoc(null).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobGroupDataService.GetSocAsync(A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeSocDeltaService.DetermineDelta(A<DeltaReportModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportDocumentService.UpsertAsync(A<DeltaReportModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'socId')", exceptionResult.Message);
        }

        [Fact]
        public async Task LmiWebhookReceiverServiceReportSocReturnsNotFound()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NotFound;
            DeltaReportModel? nullDeltaReportModel = null;

            A.CallTo(() => fakeJobGroupDataService.GetSocAsync(A<Guid>.Ignored)).Returns(nullDeltaReportModel);

            // Act
            var result = await lmiWebhookReceiverService.ReportSoc(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupDataService.GetSocAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeSocDeltaService.DetermineDelta(A<DeltaReportModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportDocumentService.UpsertAsync(A<DeltaReportModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).MustNotHaveHappened();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiWebhookReceiverServicePurgeOldReportsAsyncReturnsSuccessfully()
        {
            // Arrange
            var dummyDeltaReports = A.CollectionOfDummy<DeltaReportModel>(publishedJobGroupClientOptions.MaxReportsKept + 1);

            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).Returns(dummyDeltaReports);
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(true);

            // Act
            await lmiWebhookReceiverService.PurgeOldReportsAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappened(dummyDeltaReports.Count - publishedJobGroupClientOptions.MaxReportsKept, Times.Exactly);
        }

        [Fact]
        public async Task LmiWebhookReceiverServicePostPublishedEventAsyncReturnsSuccessfully()
        {
            // Arrange

            // Act
            await lmiWebhookReceiverService.PostPublishedEventAsync("hello world").ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }

        private static EventGridEvent[] BuildValidEventGridEvent<TModel>(string eventType, TModel? data)
            where TModel : class
        {
            var models = new EventGridEvent[]
            {
                new EventGridEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    Subject = "a-subject",
                    Data = data,
                    EventType = eventType,
                    EventTime = DateTime.Now,
                    DataVersion = "1.0",
                },
            };

            return models;
        }
    }
}
