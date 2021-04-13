using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Enums;
using DFC.Api.Lmi.Delta.Report.Functions;
using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Api.Lmi.Delta.Report.Models.FunctionRequestModels;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.FunctionTests
{
    [Trait("Category", "LmiWebhookHttpTrigger - Http trigger tests")]
    public class LmiWebhookHttpTriggerTests
    {
        private readonly ILogger<LmiWebhookHttpTrigger> fakeLogger = A.Fake<ILogger<LmiWebhookHttpTrigger>>();
        private readonly ILmiWebhookReceiverService fakeLmiWebhookReceiverService = A.Fake<ILmiWebhookReceiverService>();
        private readonly EnvironmentValues draftEnvironmentValues = new EnvironmentValues { EnvironmentNameApiSuffix = "(draft)" };
        private readonly EnvironmentValues publishedEnvironmentValues = new EnvironmentValues { EnvironmentNameApiSuffix = string.Empty };

        [Fact]
        public async Task LmiWebhookHttpTriggerPostForSubscriptionValidationReturnsOk()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.OK);
            var function = new LmiWebhookHttpTrigger(fakeLogger, draftEnvironmentValues, fakeLmiWebhookReceiverService);
            var request = BuildRequestWithValidBody("a request body");
            var webhookRequestModel = new WebhookRequestModel
            {
                WebhookCommand = WebhookCommand.SubscriptionValidation,
            };

            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).Returns(webhookRequestModel);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportAll()).MustNotHaveHappened();
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportSoc(A<Guid>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Theory]
        [InlineData(WebhookCommand.ReportDeltaForAll, 1, 0)]
        [InlineData(WebhookCommand.ReportDeltaForSoc, 0, 1)]
        public async Task LmiWebhookHttpTriggerPostForSubscriptionValidationReturnsExpectedResultCode(WebhookCommand webhookCommand, int forAllCount, int forSocCount)
        {
            // Arrange
            var expectedResult = HttpStatusCode.Created;
            var function = new LmiWebhookHttpTrigger(fakeLogger, draftEnvironmentValues, fakeLmiWebhookReceiverService);
            var request = BuildRequestWithValidBody("a request body");
            var webhookRequestModel = new WebhookRequestModel
            {
                WebhookCommand = webhookCommand,
                ContentId = Guid.NewGuid(),
            };

            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).Returns(webhookRequestModel);
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportAll()).Returns(HttpStatusCode.Created);
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportSoc(A<Guid>.Ignored)).Returns(HttpStatusCode.Created);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportAll()).MustHaveHappened(forAllCount, Times.Exactly);
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportSoc(A<Guid>.Ignored)).MustHaveHappened(forSocCount, Times.Exactly);
            var statusResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Theory]
        [InlineData(WebhookCommand.ReportDeltaForAll)]
        [InlineData(WebhookCommand.ReportDeltaForSoc)]
        public async Task LmiWebhookHttpTriggerPostForSubscriptionValidationReturnsBadRequestForPublishedEnvironment(WebhookCommand webhookCommand)
        {
            // Arrange
            var expectedResult = HttpStatusCode.BadRequest;
            var function = new LmiWebhookHttpTrigger(fakeLogger, publishedEnvironmentValues, fakeLmiWebhookReceiverService);
            var request = BuildRequestWithValidBody("a request body");
            var webhookRequestModel = new WebhookRequestModel
            {
                WebhookCommand = webhookCommand,
            };

            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).Returns(webhookRequestModel);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportAll()).MustNotHaveHappened();
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportSoc(A<Guid>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task LmiWebhookHttpTriggerPostForSubscriptionValidationReturnsBadRequest()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.BadRequest);
            var function = new LmiWebhookHttpTrigger(fakeLogger, draftEnvironmentValues, fakeLmiWebhookReceiverService);
            var request = BuildRequestWithValidBody("a request body");
            var webhookRequestModel = new WebhookRequestModel
            {
                WebhookCommand = WebhookCommand.None,
            };

            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).Returns(webhookRequestModel);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportAll()).MustNotHaveHappened();
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportSoc(A<Guid>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task LmiWebhookHttpTriggerPostWithNoBodyReturnsBadRequest()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.BadRequest);
            var function = new LmiWebhookHttpTrigger(fakeLogger, draftEnvironmentValues, fakeLmiWebhookReceiverService);
            var request = BuildRequestWithValidBody(string.Empty);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportAll()).MustNotHaveHappened();
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportSoc(A<Guid>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task LmiWebhookHttpTriggerPostCatchesException()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            var function = new LmiWebhookHttpTrigger(fakeLogger, draftEnvironmentValues, fakeLmiWebhookReceiverService);
            var request = BuildRequestWithValidBody("a request body");

            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).Throws(new Exception());

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportAll()).MustNotHaveHappened();
            A.CallTo(() => fakeLmiWebhookReceiverService.ReportSoc(A<Guid>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        private static HttpRequest BuildRequestWithValidBody(string bodyString)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyString)),
            };
        }
    }
}
