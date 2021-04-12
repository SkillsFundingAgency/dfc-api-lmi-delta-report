using AutoMapper;
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
            const int expectedSocDeltaCount = 1;
            var fullDeltaReportModel = new FullDeltaReportModel
            {
                DeltaReportSocs = new List<DeltaReportSocModel>
                {
                    new DeltaReportSocModel
                    {
                        Soc = 1234,
                        PublishedJobGroup = new JobGroupModel(),
                        DraftJobGroup = new JobGroupModel(),
                    },
                    new DeltaReportSocModel
                    {
                        Soc = 4321,
                        PublishedJobGroup = new JobGroupModel(),
                        DraftJobGroup = new JobGroupModel(),
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
            Assert.Equal(expectedSocDeltaCount, fullDeltaReportModel.SocDeltaCount);
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
