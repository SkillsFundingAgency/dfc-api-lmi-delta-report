using DFC.Api.Lmi.Delta.Report.Connectors;
using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Api.Lmi.Delta.Report.Models.ClientOptions;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.ConnectorTests
{
    [Trait("Category", "Published job groups API connector Unit Tests")]
    public class PublishedJobGroupApiConnectorTests
    {
        private readonly ILogger<PublishedJobGroupApiConnector> fakeLogger = A.Fake<ILogger<PublishedJobGroupApiConnector>>();
        private readonly HttpClient httpClient = new HttpClient();
        private readonly IApiDataConnector fakeApiDataConnector = A.Fake<IApiDataConnector>();
        private readonly PublishedJobGroupApiConnector publishedJobGroupApiConnector;
        private readonly PublishedJobGroupClientOptions publishedJobGroupClientOptions = new PublishedJobGroupClientOptions
        {
            BaseAddress = new Uri("https://somewhere.com", UriKind.Absolute),
        };

        public PublishedJobGroupApiConnectorTests()
        {
            publishedJobGroupApiConnector = new PublishedJobGroupApiConnector(fakeLogger, httpClient, fakeApiDataConnector, publishedJobGroupClientOptions);
        }

        [Fact]
        public async Task JobGroupApiConnectorTestsGetSummaryReturnsSuccess()
        {
            // arrange
            var expectedResults = new List<JobGroupSummaryItemModel>
            {
                new JobGroupSummaryItemModel
                {
                    Id = Guid.NewGuid(),
                    Soc = 3543,
                    Title = "A title",
                },
            };

            A.CallTo(() => fakeApiDataConnector.GetAsync<List<JobGroupSummaryItemModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(expectedResults);

            // act
            var results = await publishedJobGroupApiConnector.GetSummaryAsync().ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiDataConnector.GetAsync<List<JobGroupSummaryItemModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.NotNull(results);
            Assert.Equal(expectedResults.Count, results!.Count);
            Assert.Equal(expectedResults.First().Soc, results.First().Soc);
            Assert.Equal(expectedResults.First().Title, results.First().Title);
        }

        [Fact]
        public async Task JobGroupApiConnectorTestsGetDetailForSocReturnsSuccess()
        {
            // arrange
            var expectedResult = new JobGroupModel
            {
                Soc = 3543,
                Title = "A title",
            };

            A.CallTo(() => fakeApiDataConnector.GetAsync<JobGroupModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(expectedResult);

            // act
            var result = await publishedJobGroupApiConnector.GetDetailAsync(1234).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiDataConnector.GetAsync<JobGroupModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.NotNull(result);
            Assert.Equal(expectedResult.Soc, result?.Soc);
            Assert.Equal(expectedResult.Title, result?.Title);
        }

        [Fact]
        public async Task JobGroupApiConnectorTestsGetDetailForIdReturnsSuccess()
        {
            // arrange
            var expectedResult = new JobGroupModel
            {
                Soc = 3543,
                Title = "A title",
            };

            A.CallTo(() => fakeApiDataConnector.GetAsync<JobGroupModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(expectedResult);

            // act
            var result = await publishedJobGroupApiConnector.GetDetailAsync(Guid.NewGuid()).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiDataConnector.GetAsync<JobGroupModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.NotNull(result);
            Assert.Equal(expectedResult.Soc, result?.Soc);
            Assert.Equal(expectedResult.Title, result?.Title);
        }
    }
}
