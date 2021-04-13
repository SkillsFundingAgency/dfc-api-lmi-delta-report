using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Api.Lmi.Delta.Report.Models.ClientOptions;
using DFC.Api.Lmi.Delta.Report.Services;
using FakeItEasy;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.ServiceTests
{
    [Trait("Category", "EventGridService - service Unit Tests")]
    public class EventGridServiceTests
    {
        private readonly Logger<EventGridService> fakeLogger = A.Fake<Logger<EventGridService>>();
        private readonly IEventGridClientService fakeEventGridClientService = A.Fake<IEventGridClientService>();
        private readonly IEventGridService eventGridService;
        private readonly EventGridClientOptions eventGridClientOptions = new EventGridClientOptions
        {
            ApiEndpoint = new Uri("https://somewhere.com", UriKind.Absolute),
            SubjectPrefix = "SubjectPrefix",
            TopicEndpoint = "TopicEndpoint",
            TopicKey = "TopicKey",
        };

        public EventGridServiceTests()
        {
            eventGridService = new EventGridService(fakeLogger, fakeEventGridClientService, eventGridClientOptions);
        }

        [Fact]
        public async Task EventGridServiceSendEventReturnsSuccess()
        {
            // arrange
            var dummyEventGridEventData = A.Dummy<EventGridEventData>();

            // act
            await eventGridService.SendEventAsync(dummyEventGridEventData, "a subject", "an event type").ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeEventGridClientService.SendEventAsync(A<List<EventGridEvent>>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task EventGridServiceSendEventWithInvalidOptionsDoesNothing()
        {
            // arrange
            var dummyEventGridEventData = A.Dummy<EventGridEventData>();
            eventGridClientOptions.ApiEndpoint = null;

            // act
            await eventGridService.SendEventAsync(dummyEventGridEventData, "a subject", "an event type").ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeEventGridClientService.SendEventAsync(A<List<EventGridEvent>>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task EventGridServiceSendEventThrowsExceoptionWhenNullEventGridEventData()
        {
            // arrange
            EventGridEventData? nullEventGridEventData = null;

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await eventGridService.SendEventAsync(nullEventGridEventData, "a subject", "an event type").ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeEventGridClientService.SendEventAsync(A<List<EventGridEvent>>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'eventGridEventData')", exceptionResult.Message);
        }

        [Fact]
        public void EventGridServiceIsValidEventGridClientOptionsReturnsSuccess()
        {
            // arrange
            const bool expectedResult = true;

            // act
            var result = eventGridService.IsValidEventGridClientOptions(eventGridClientOptions);

            // assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void EventGridServiceIsValidEventGridClientOptionsReturnsFalseWhenNullTopicEndpoint()
        {
            // arrange
            const bool expectedResult = false;

            eventGridClientOptions.TopicEndpoint = string.Empty;

            // act
            var result = eventGridService.IsValidEventGridClientOptions(eventGridClientOptions);

            // assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void EventGridServiceIsValidEventGridClientOptionsReturnsFalsenWhenNullTopicKey()
        {
            // arrange
            const bool expectedResult = false;

            eventGridClientOptions.TopicKey = string.Empty;

            // act
            var result = eventGridService.IsValidEventGridClientOptions(eventGridClientOptions);

            // assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void EventGridServiceIsValidEventGridClientOptionsReturnsFalseWhenNullSubjectPrefix()
        {
            // arrange
            const bool expectedResult = false;

            eventGridClientOptions.SubjectPrefix = string.Empty;

            // act
            var result = eventGridService.IsValidEventGridClientOptions(eventGridClientOptions);

            // assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void EventGridServiceIsValidEventGridClientOptionsReturnsFalseWhenNullApiEndpoint()
        {
            // arrange
            const bool expectedResult = false;

            eventGridClientOptions.ApiEndpoint = null;

            // act
            var result = eventGridService.IsValidEventGridClientOptions(eventGridClientOptions);

            // assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void EventGridServiceIsValidEventGridClientOptionsRaisesExceptionWhenNullEventGridClientOptions()
        {
            // arrange

            // act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => eventGridService.IsValidEventGridClientOptions(null));

            // assert
            Assert.Equal("Value cannot be null. (Parameter 'eventGridClientOptions')", exceptionResult.Message);
        }
    }
}
