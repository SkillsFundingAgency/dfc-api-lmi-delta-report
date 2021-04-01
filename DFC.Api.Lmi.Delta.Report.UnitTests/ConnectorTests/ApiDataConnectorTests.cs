using DFC.Api.Lmi.Delta.Report.Connectors;
using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.UnitTests.TestModels;
using FakeItEasy;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.ConnectorTests
{
    [Trait("Category", "API data connector Unit Tests")]
    public class ApiDataConnectorTests
    {
        private readonly Uri dummyUrl = new Uri("https://somewhere.com", UriKind.Absolute);
        private readonly IApiConnector fakeApiConnector = A.Fake<IApiConnector>();
        private readonly IApiDataConnector apiDataConnector;

        public ApiDataConnectorTests()
        {
            apiDataConnector = new ApiDataConnector(fakeApiConnector);
        }

        [Fact]
        public async Task ApiDataConnectorGetReturnsSuccess()
        {
            // arrange
            var expectedResult = new ApiTestModel
            {
                Id = Guid.NewGuid(),
                Name = "a name",
            };
            var jsonResponse = JsonConvert.SerializeObject(expectedResult);

            A.CallTo(() => fakeApiConnector.GetAsync(A<HttpClient>.Ignored, A<Uri>.Ignored, A<string>.Ignored)).Returns(jsonResponse);

            // act
            var result = await apiDataConnector.GetAsync<ApiTestModel>(A.Fake<HttpClient>(), dummyUrl).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiConnector.GetAsync(A<HttpClient>.Ignored, A<Uri>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.NotNull(result);
            Assert.Equal(expectedResult.Id, result!.Id);
            Assert.Equal(expectedResult.Name, result.Name);
        }

        [Fact]
        public async Task ApiDataConnectorGetReturnsNullForNoData()
        {
            // arrange
            ApiTestModel? expectedResult = null;

            A.CallTo(() => fakeApiConnector.GetAsync(A<HttpClient>.Ignored, A<Uri>.Ignored, A<string>.Ignored)).Returns(string.Empty);

            // act
            var result = await apiDataConnector.GetAsync<ApiTestModel>(A.Fake<HttpClient>(), dummyUrl).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiConnector.GetAsync(A<HttpClient>.Ignored, A<Uri>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(result, expectedResult);
        }

        [Fact]
        public async Task ApiDataConnectorGetReturnsExceptionForNullHttpClient()
        {
            // arrange

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await apiDataConnector.GetAsync<ApiTestModel>(null, dummyUrl).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiConnector.GetAsync(A<HttpClient>.Ignored, A<Uri>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'httpClient')", exceptionResult.Message);
        }
    }
}
