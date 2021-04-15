using AutoMapper;
using DFC.Api.Lmi.Delta.Report.Common;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using DFC.Api.Lmi.Delta.Report.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.ServiceTests
{
    [Trait("Category", "SocDeltaService - service Unit Tests")]
    public class SocDeltaServiceTests
    {
        private readonly ILogger<SocDeltaService> fakeLogger = A.Fake<ILogger<SocDeltaService>>();
        private readonly IMapper fakeMapper = A.Fake<IMapper>();
        private readonly SocDeltaService socDeltaService;

        public SocDeltaServiceTests()
        {
            socDeltaService = new SocDeltaService(fakeLogger, fakeMapper);
        }

        [Fact]
        public void SocDeltaServiceTestsDetermineDeltaIsSuccessful()
        {
            // Arrange
            const int expectedSocAdditionCount = 3;
            const int expectedSocUpdateCount = 1;
            const int expectedSocDeletionCount = 2;
            var fullDeltaReportModel = new FullDeltaReportModel
            {
                DeltaReportSocs = new List<DeltaReportSocModel>
                {
                    new DeltaReportSocModel
                    {
                        Soc = 1234,
                        State = DeltaReportState.Updated,
                        PublishedJobGroup = new JobGroupModel(),
                        DraftJobGroup = new JobGroupModel(),
                    },
                    new DeltaReportSocModel
                    {
                        Soc = 1111,
                        State = DeltaReportState.Addition,
                        PublishedJobGroup = null,
                        DraftJobGroup = new JobGroupModel(),
                    },
                    new DeltaReportSocModel
                    {
                        Soc = 1112,
                        State = DeltaReportState.Addition,
                        PublishedJobGroup = null,
                        DraftJobGroup = new JobGroupModel(),
                    },
                    new DeltaReportSocModel
                    {
                        Soc = 1113,
                        State = DeltaReportState.Addition,
                        PublishedJobGroup = null,
                        DraftJobGroup = new JobGroupModel(),
                    },
                    new DeltaReportSocModel
                    {
                        Soc = 2221,
                        State = DeltaReportState.Unchanged,
                        PublishedJobGroup = new JobGroupModel(),
                        DraftJobGroup = new JobGroupModel(),
                    },
                    new DeltaReportSocModel
                    {
                        Soc = 3331,
                        State = DeltaReportState.Deletion,
                        PublishedJobGroup = new JobGroupModel(),
                        DraftJobGroup = null,
                    },
                    new DeltaReportSocModel
                    {
                        Soc = 3332,
                        State = DeltaReportState.Deletion,
                        PublishedJobGroup = new JobGroupModel(),
                        DraftJobGroup = null,
                    },
                },
            };
            var publishedJobGroupToDeltaModel = new JobGroupToDeltaModel
            {
                Description = "this is the published description",
            };
            var draftJobGroupToDelta = new JobGroupToDeltaModel
            {
                Description = "this is the draft description",
            };
            A.CallTo(() => fakeMapper.Map<JobGroupToDeltaModel>(fullDeltaReportModel.DeltaReportSocs.First().PublishedJobGroup)).Returns(publishedJobGroupToDeltaModel);
            A.CallTo(() => fakeMapper.Map<JobGroupToDeltaModel>(fullDeltaReportModel.DeltaReportSocs.First().DraftJobGroup)).Returns(draftJobGroupToDelta);

            // Act
            socDeltaService.DetermineDelta(fullDeltaReportModel);

            // Assert
            A.CallTo(() => fakeMapper.Map<JobGroupToDeltaModel>(fullDeltaReportModel.DeltaReportSocs.First().PublishedJobGroup)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<JobGroupToDeltaModel>(fullDeltaReportModel.DeltaReportSocs.First().DraftJobGroup)).MustHaveHappenedOnceExactly();
            Assert.Equal(fullDeltaReportModel.DeltaReportSocs.Count, fullDeltaReportModel.SocImportedCount);
            Assert.Equal(expectedSocAdditionCount, fullDeltaReportModel.SocAdditionCount);
            Assert.Equal(expectedSocUpdateCount, fullDeltaReportModel.SocUpdateCount);
            Assert.Equal(expectedSocDeletionCount, fullDeltaReportModel.SocDeletionCount);
        }

        [Fact]
        public void SocDeltaServiceTestsDetermineDeltaThrowsExceoptionWhenNullDeltaReportModel()
        {
            // Arrange

            // Act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => socDeltaService.DetermineDelta(null));

            // assert
            Assert.Equal("Value cannot be null. (Parameter 'fullDeltaReportModel')", exceptionResult.Message);
        }
    }
}
