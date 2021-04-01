using DFC.Api.Lmi.Delta.Report.Connectors;
using DFC.Api.Lmi.Delta.Report.UnitTests.FakeHttpHandlers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.ConnectorTests
{
    [Trait("Category", "API connector Unit Tests")]
    public class ApiConnectorTests
    {
        private readonly Uri dummyUrl = new Uri("https://somewhere.com", UriKind.Absolute);
        private readonly ILogger<ApiConnector> fakeLogger = A.Fake<ILogger<ApiConnector>>();
        private readonly ApiConnector apiConnector;

        public ApiConnectorTests()
        {
            apiConnector = new ApiConnector(fakeLogger);
        }

        [Fact]
        public async Task ApiConnectorGetReturnsOkStatusCodeForValidUrl()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            const string expectedResponse = "Expected response string";
            using var httpResponse = new HttpResponseMessage { StatusCode = expectedResult, Content = new StringContent(expectedResponse) };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler);

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            // act
            var result = await apiConnector.GetAsync(httpClient, dummyUrl, MediaTypeNames.Application.Json).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task ApiConnectorGetReturnsNotFoundStatusCode()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NotFound;
            string expectedResponse = string.Empty;
            using var httpResponse = new HttpResponseMessage { StatusCode = expectedResult, Content = new StringContent(expectedResponse) };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler);

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            // act
            var result = await apiConnector.GetAsync(httpClient, dummyUrl, MediaTypeNames.Application.Json).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Null(result);
        }

        [Fact]
        public async Task ApiConnectorGetReturnsNoContentStatusCode()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            string expectedResponse = string.Empty;
            using var httpResponse = new HttpResponseMessage { StatusCode = expectedResult, Content = new StringContent(expectedResponse) };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler);

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            // act
            var result = await apiConnector.GetAsync(httpClient, dummyUrl, MediaTypeNames.Application.Json).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Null(result);
        }

        [Fact]
        public async Task ApiConnectorGetReturnsExceptionResult()
        {
            // arrange
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler);

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Throws(new ArgumentException("fake exception"));

            // act
            var result = await apiConnector.GetAsync(httpClient, dummyUrl, MediaTypeNames.Application.Json).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Null(result);
        }

        [Fact]
        public async Task ApiConnectorGetReturnsExceptionForNullHttpClient()
        {
            // arrange
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await apiConnector.GetAsync(null, dummyUrl, MediaTypeNames.Application.Json).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'httpClient')", exceptionResult.Message);
        }
    }
}
