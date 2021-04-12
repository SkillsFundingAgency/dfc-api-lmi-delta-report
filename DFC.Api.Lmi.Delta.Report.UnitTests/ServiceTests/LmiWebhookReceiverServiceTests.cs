using AutoMapper;
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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly IMapper fakeMapper = A.Fake<IMapper>();
        private readonly IDocumentService<DeltaReportModel> fakeDeltaReportDocumentService = A.Fake<IDocumentService<DeltaReportModel>>();
        private readonly IDocumentService<DeltaReportSocModel> fakeDeltaReportSocDocumentService = A.Fake<IDocumentService<DeltaReportSocModel>>();
        private readonly IJobGroupDataService fakeJobGroupDataService = A.Fake<IJobGroupDataService>();
        private readonly ISocDeltaService fakeSocDeltaService = A.Fake<ISocDeltaService>();
        private readonly IEventGridService fakeEventGridService = A.Fake<IEventGridService>();
        private readonly LmiWebhookReceiverService lmiWebhookReceiverService;
        private readonly PublishedJobGroupClientOptions publishedJobGroupClientOptions = new PublishedJobGroupClientOptions();
        private readonly EventGridClientOptions eventGridClientOptions = new EventGridClientOptions { ApiEndpoint = new Uri("https://somewhere.com", UriKind.Absolute) };

        public LmiWebhookReceiverServiceTests()
        {
            lmiWebhookReceiverService = new LmiWebhookReceiverService(fakeLogger, fakeMapper, fakeDeltaReportDocumentService, fakeDeltaReportSocDocumentService, fakeJobGroupDataService, fakeSocDeltaService, fakeEventGridService, publishedJobGroupClientOptions, eventGridClientOptions);
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
            var dummyFullDeltaReportModel = A.Dummy<FullDeltaReportModel>();
            var dummyDeltaReports = A.CollectionOfDummy<DeltaReportModel>(publishedJobGroupClientOptions.MaxReportsKept + 1);
            var dummyDeltaReportSocs = A.CollectionOfDummy<DeltaReportSocModel>(1);
            dummyFullDeltaReportModel.DeltaReportSocs = A.CollectionOfFake<DeltaReportSocModel>(1).ToList();
            A.CallTo(() => fakeJobGroupDataService.GetAllAsync()).Returns(dummyFullDeltaReportModel);
            A.CallTo(() => fakeDeltaReportDocumentService.UpsertAsync(A<DeltaReportModel>.Ignored)).Returns(HttpStatusCode.Created);
            A.CallTo(() => fakeDeltaReportSocDocumentService.UpsertAsync(A<DeltaReportSocModel>.Ignored)).Returns(HttpStatusCode.Created);
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).Returns(dummyDeltaReports);
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(true);
            A.CallTo(() => fakeDeltaReportSocDocumentService.GetAsync(A<Expression<Func<DeltaReportSocModel, bool>>>.Ignored)).Returns(dummyDeltaReportSocs);
            A.CallTo(() => fakeDeltaReportSocDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(true);

            // Act
            var result = await lmiWebhookReceiverService.ReportAll().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupDataService.GetAllAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeSocDeltaService.DetermineDelta(A<FullDeltaReportModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.UpsertAsync(A<DeltaReportModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportSocDocumentService.UpsertAsync(A<DeltaReportSocModel>.Ignored)).MustHaveHappened(dummyFullDeltaReportModel.DeltaReportSocs!.Count, Times.Exactly);
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappened(dummyDeltaReports.Count - publishedJobGroupClientOptions.MaxReportsKept, Times.Exactly);
            A.CallTo(() => fakeDeltaReportSocDocumentService.GetAsync(A<Expression<Func<DeltaReportSocModel, bool>>>.Ignored)).MustHaveHappened(dummyDeltaReports.Count - publishedJobGroupClientOptions.MaxReportsKept, Times.Exactly);
            A.CallTo(() => fakeDeltaReportSocDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappened((dummyDeltaReports.Count - publishedJobGroupClientOptions.MaxReportsKept) * dummyDeltaReportSocs.Count, Times.Exactly);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiWebhookReceiverServiceReportSocReturnsSuccessfully()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.Created;
            var dummyFullDeltaReportModel = A.Dummy<FullDeltaReportModel>();
            var dummyDeltaReports = A.CollectionOfDummy<DeltaReportModel>(publishedJobGroupClientOptions.MaxReportsKept + 1);
            var dummyDeltaReportSocs = A.CollectionOfDummy<DeltaReportSocModel>(1);
            dummyFullDeltaReportModel.DeltaReportSocs = A.CollectionOfFake<DeltaReportSocModel>(1).ToList();
            A.CallTo(() => fakeJobGroupDataService.GetSocAsync(A<Guid>.Ignored)).Returns(dummyFullDeltaReportModel);
            A.CallTo(() => fakeDeltaReportDocumentService.UpsertAsync(A<DeltaReportModel>.Ignored)).Returns(HttpStatusCode.Created);
            A.CallTo(() => fakeDeltaReportSocDocumentService.UpsertAsync(A<DeltaReportSocModel>.Ignored)).Returns(HttpStatusCode.Created);
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).Returns(dummyDeltaReports);
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(true);
            A.CallTo(() => fakeDeltaReportSocDocumentService.GetAsync(A<Expression<Func<DeltaReportSocModel, bool>>>.Ignored)).Returns(dummyDeltaReportSocs);
            A.CallTo(() => fakeDeltaReportSocDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(true);

            // Act
            var result = await lmiWebhookReceiverService.ReportSoc(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupDataService.GetSocAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeSocDeltaService.DetermineDelta(A<FullDeltaReportModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.UpsertAsync(A<DeltaReportModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportSocDocumentService.UpsertAsync(A<DeltaReportSocModel>.Ignored)).MustHaveHappened(dummyFullDeltaReportModel.DeltaReportSocs!.Count, Times.Exactly);
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappened(dummyDeltaReports.Count - publishedJobGroupClientOptions.MaxReportsKept, Times.Exactly);
            A.CallTo(() => fakeDeltaReportSocDocumentService.GetAsync(A<Expression<Func<DeltaReportSocModel, bool>>>.Ignored)).MustHaveHappened(dummyDeltaReports.Count - publishedJobGroupClientOptions.MaxReportsKept, Times.Exactly);
            A.CallTo(() => fakeDeltaReportSocDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappened((dummyDeltaReports.Count - publishedJobGroupClientOptions.MaxReportsKept) * dummyDeltaReportSocs.Count, Times.Exactly);
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
            A.CallTo(() => fakeSocDeltaService.DetermineDelta(A<FullDeltaReportModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportDocumentService.UpsertAsync(A<DeltaReportModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportSocDocumentService.GetAllAsync(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportSocDocumentService.DeleteAsync(A<Guid>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'socId')", exceptionResult.Message);
        }

        [Fact]
        public async Task LmiWebhookReceiverServiceReportSocReturnsNotFound()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NotFound;
            FullDeltaReportModel? nullFullDeltaReportModel = null;

            A.CallTo(() => fakeJobGroupDataService.GetSocAsync(A<Guid>.Ignored)).Returns(nullFullDeltaReportModel);

            // Act
            var result = await lmiWebhookReceiverService.ReportSoc(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupDataService.GetSocAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeSocDeltaService.DetermineDelta(A<FullDeltaReportModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportDocumentService.UpsertAsync(A<DeltaReportModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportSocDocumentService.GetAllAsync(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDeltaReportSocDocumentService.DeleteAsync(A<Guid>.Ignored)).MustNotHaveHappened();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiWebhookReceiverServicePurgeOldReportsAsyncReturnsSuccessfully()
        {
            // Arrange
            var dummyDeltaReports = A.CollectionOfDummy<DeltaReportModel>(publishedJobGroupClientOptions.MaxReportsKept + 1);
            var dummyDeltaReportSocs = A.CollectionOfDummy<DeltaReportSocModel>(1);

            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).Returns(dummyDeltaReports);
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(true);
            A.CallTo(() => fakeDeltaReportSocDocumentService.GetAsync(A<Expression<Func<DeltaReportSocModel, bool>>>.Ignored)).Returns(dummyDeltaReportSocs);
            A.CallTo(() => fakeDeltaReportSocDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(true);

            // Act
            await lmiWebhookReceiverService.PurgeOldReportsAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDeltaReportDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDeltaReportDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappened(dummyDeltaReports.Count - publishedJobGroupClientOptions.MaxReportsKept, Times.Exactly);
            A.CallTo(() => fakeDeltaReportSocDocumentService.GetAsync(A<Expression<Func<DeltaReportSocModel, bool>>>.Ignored)).MustHaveHappened(dummyDeltaReports.Count - publishedJobGroupClientOptions.MaxReportsKept, Times.Exactly);
            A.CallTo(() => fakeDeltaReportSocDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappened((dummyDeltaReports.Count - publishedJobGroupClientOptions.MaxReportsKept) * dummyDeltaReportSocs.Count, Times.Exactly);
        }

        [Fact]
        public async Task LmiWebhookReceiverServiceSaveDeltaReportSocsReturnsCreatedSuccessfully()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.Created;
            var dummyDeltaReportSocs = A.CollectionOfDummy<DeltaReportSocModel>(2).ToList();

            A.CallTo(() => fakeDeltaReportSocDocumentService.UpsertAsync(A<DeltaReportSocModel>.Ignored)).Returns(HttpStatusCode.Created);

            // Act
            var result = await lmiWebhookReceiverService.SaveDeltaReportSocs(dummyDeltaReportSocs).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDeltaReportSocDocumentService.UpsertAsync(A<DeltaReportSocModel>.Ignored)).MustHaveHappened(dummyDeltaReportSocs.Count, Times.Exactly);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiWebhookReceiverServiceSaveDeltaReportSocsReturnsNoContent()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            var dummyDeltaReportSocs = A.CollectionOfDummy<DeltaReportSocModel>(0).ToList();

            // Act
            var result = await lmiWebhookReceiverService.SaveDeltaReportSocs(dummyDeltaReportSocs).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDeltaReportSocDocumentService.UpsertAsync(A<DeltaReportSocModel>.Ignored)).MustNotHaveHappened();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiWebhookReceiverServiceSaveDeltaReportSocsReturnsBadRequest()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var dummyDeltaReportSocs = A.CollectionOfDummy<DeltaReportSocModel>(1).ToList();

            A.CallTo(() => fakeDeltaReportSocDocumentService.UpsertAsync(A<DeltaReportSocModel>.Ignored)).Returns(HttpStatusCode.BadRequest);

            // Act
            var result = await lmiWebhookReceiverService.SaveDeltaReportSocs(dummyDeltaReportSocs).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDeltaReportSocDocumentService.UpsertAsync(A<DeltaReportSocModel>.Ignored)).MustHaveHappened(dummyDeltaReportSocs.Count, Times.Exactly);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiWebhookReceiverServicePostPublishedEventAsyncReturnsSuccessfully()
        {
            // Arrange

            // Act
            await lmiWebhookReceiverService.PostPublishedEventAsync("hello world", new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

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
