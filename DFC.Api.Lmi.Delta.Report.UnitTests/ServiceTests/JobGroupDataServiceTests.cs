using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using DFC.Api.Lmi.Delta.Report.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.ServiceTests
{
    [Trait("Category", "JobGroupDataService - service Unit Tests")]
    public class JobGroupDataServiceTests
    {
        private const int Soc1111 = 1111;
        private const int Soc2222 = 2222;
        private const int Soc3333 = 3333;

        private readonly List<JobGroupSummaryItemModel> draftJobGroupSummaryItemModels;
        private readonly List<JobGroupSummaryItemModel> publishedJobGroupSummaryItemModels;
        private readonly JobGroupModel dummyDraftJobGroupModel01;
        private readonly JobGroupModel dummyDraftJobGroupModel02;
        private readonly JobGroupModel dummyPublishedJobGroupModel02;
        private readonly JobGroupModel dummyPublishedJobGroupModel03;
        private readonly JobGroupModel[] dummyDraftJobGroupModels;
        private readonly JobGroupModel[] dummyPublishedJobGroupModels;
        private readonly DeltaReportModel expectedGetAllResult;
        private readonly DeltaReportModel expectedGetSocResult;

        private readonly ILogger<JobGroupDataService> fakeLogger = A.Fake<ILogger<JobGroupDataService>>();
        private readonly IDraftJobGroupApiConnector fakeDraftJobGroupApiConnector = A.Fake<IDraftJobGroupApiConnector>();
        private readonly IPublishedJobGroupApiConnector fakePublishedJobGroupApiConnector = A.Fake<IPublishedJobGroupApiConnector>();
        private readonly JobGroupDataService jobGroupDataService;

        public JobGroupDataServiceTests()
        {
            jobGroupDataService = new JobGroupDataService(fakeLogger, fakeDraftJobGroupApiConnector, fakePublishedJobGroupApiConnector);

            draftJobGroupSummaryItemModels = new List<JobGroupSummaryItemModel>
            {
                new JobGroupSummaryItemModel
                {
                    Soc = Soc1111,
                },
                new JobGroupSummaryItemModel
                {
                    Soc = Soc2222,
                },
            };
            publishedJobGroupSummaryItemModels = new List<JobGroupSummaryItemModel>
            {
                new JobGroupSummaryItemModel
                {
                    Soc = Soc2222,
                },
                new JobGroupSummaryItemModel
                {
                    Soc = Soc3333,
                },
            };
            dummyDraftJobGroupModel01 = new JobGroupModel
            {
                Soc = Soc1111,
            };
            dummyDraftJobGroupModel02 = new JobGroupModel
            {
                Soc = Soc2222,
            };
            dummyPublishedJobGroupModel02 = new JobGroupModel
            {
                Soc = Soc2222,
            };
            dummyPublishedJobGroupModel03 = new JobGroupModel
            {
                Soc = Soc3333,
            };
            dummyDraftJobGroupModels = new[] { dummyDraftJobGroupModel01, dummyDraftJobGroupModel02 };
            dummyPublishedJobGroupModels = new[] { dummyPublishedJobGroupModel02, dummyPublishedJobGroupModel03 };
            expectedGetAllResult = new DeltaReportModel
            {
                DeltaReportSocs = new List<DeltaReportSocModel>
                {
                    new DeltaReportSocModel
                    {
                         Soc = Soc1111,
                         DraftJobGroup = dummyDraftJobGroupModel01,
                         PublishedJobGroup = null,
                    },
                    new DeltaReportSocModel
                    {
                         Soc = Soc2222,
                         DraftJobGroup = dummyDraftJobGroupModel02,
                         PublishedJobGroup = dummyPublishedJobGroupModel02,
                    },
                    new DeltaReportSocModel
                    {
                         Soc = Soc3333,
                         DraftJobGroup = null,
                         PublishedJobGroup = dummyPublishedJobGroupModel03,
                    },
                },
            };
            expectedGetSocResult = new DeltaReportModel
            {
                DeltaReportSocs = new List<DeltaReportSocModel>
                {
                    new DeltaReportSocModel
                    {
                         Soc = Soc2222,
                         DraftJobGroup = dummyDraftJobGroupModel02,
                         PublishedJobGroup = dummyPublishedJobGroupModel02,
                    },
                },
            };
        }

        [Fact]
        public async Task JobGroupDataServiceGetAllReturnsSuccessfully()
        {
            A.CallTo(() => fakeDraftJobGroupApiConnector.GetSummaryAsync()).Returns(draftJobGroupSummaryItemModels);
            A.CallTo(() => fakePublishedJobGroupApiConnector.GetSummaryAsync()).Returns(publishedJobGroupSummaryItemModels);
            A.CallTo(() => fakeDraftJobGroupApiConnector.GetDetailAsync(A<int>.Ignored)).ReturnsNextFromSequence(dummyDraftJobGroupModels);
            A.CallTo(() => fakePublishedJobGroupApiConnector.GetDetailAsync(A<int>.Ignored)).ReturnsNextFromSequence(dummyPublishedJobGroupModels);

            // Act
            var result = await jobGroupDataService.GetAllAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDraftJobGroupApiConnector.GetSummaryAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakePublishedJobGroupApiConnector.GetSummaryAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDraftJobGroupApiConnector.GetDetailAsync(A<int>.Ignored)).MustHaveHappened(expectedGetAllResult.DeltaReportSocs!.Count, Times.Exactly);
            A.CallTo(() => fakePublishedJobGroupApiConnector.GetDetailAsync(A<int>.Ignored)).MustHaveHappened(expectedGetAllResult.DeltaReportSocs.Count, Times.Exactly);
            Assert.NotNull(result.DeltaReportSocs);
            Assert.Equal(expectedGetAllResult.DeltaReportSocs.Count, result.DeltaReportSocs?.Count);
            Assert.NotNull(result?.DeltaReportSocs?[0].PublishedJobGroup);
            Assert.NotNull(result?.DeltaReportSocs?[2].DraftJobGroup);
            Assert.Equal(expectedGetAllResult.DeltaReportSocs[0].Soc, result?.DeltaReportSocs?[0].Soc);
            Assert.Equal(expectedGetAllResult.DeltaReportSocs[1].Soc, result?.DeltaReportSocs?[1].Soc);
            Assert.Equal(expectedGetAllResult.DeltaReportSocs[2].Soc, result?.DeltaReportSocs?[2].Soc);
        }

        [Fact]
        public async Task JobGroupDataServiceGetSocReturnsSuccessfully()
        {
            // Arrange
            A.CallTo(() => fakeDraftJobGroupApiConnector.GetDetailAsync(A<Guid>.Ignored)).Returns(dummyDraftJobGroupModel02);
            A.CallTo(() => fakePublishedJobGroupApiConnector.GetDetailAsync(A<int>.Ignored)).Returns(dummyPublishedJobGroupModel02);

            // Act
            var result = await jobGroupDataService.GetSocAsync(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDraftJobGroupApiConnector.GetDetailAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakePublishedJobGroupApiConnector.GetDetailAsync(A<int>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.NotNull(result?.DeltaReportSocs);
            Assert.Equal(expectedGetSocResult.DeltaReportSocs?.Count, result?.DeltaReportSocs?.Count);
            Assert.NotNull(result?.DeltaReportSocs?[0].PublishedJobGroup);
            Assert.NotNull(result?.DeltaReportSocs?[0].DraftJobGroup);
            Assert.Equal(expectedGetSocResult.DeltaReportSocs?[0].Soc, result?.DeltaReportSocs?[0].Soc);
        }

        [Fact]
        public async Task JobGroupDataServiceGetSocReturnsNullForNoData()
        {
            // Arrange
            JobGroupModel? nullJobGroupModel = null;
            A.CallTo(() => fakeDraftJobGroupApiConnector.GetDetailAsync(A<Guid>.Ignored)).Returns(nullJobGroupModel);

            // Act
            var result = await jobGroupDataService.GetSocAsync(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDraftJobGroupApiConnector.GetDetailAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakePublishedJobGroupApiConnector.GetDetailAsync(A<int>.Ignored)).MustNotHaveHappened();
            Assert.Null(result);
        }

        [Fact]
        public async Task JobGroupDataServiceGetSocReturnsExceptionForNullSocId()
        {
            // Arrange

            // Act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await jobGroupDataService.GetSocAsync(null).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDraftJobGroupApiConnector.GetDetailAsync(A<Guid>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakePublishedJobGroupApiConnector.GetDetailAsync(A<int>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'socId')", exceptionResult.Message);
        }
    }
}
