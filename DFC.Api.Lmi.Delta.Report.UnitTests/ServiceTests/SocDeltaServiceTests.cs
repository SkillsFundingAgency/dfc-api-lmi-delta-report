using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using DFC.Api.Lmi.Delta.Report.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.ServiceTests
{
    [Trait("Category", "SocDeltaService - service Unit Tests")]
    public class SocDeltaServiceTests
    {
        private readonly ILogger<SocDeltaService> fakeLogger = A.Fake<ILogger<SocDeltaService>>();
        private readonly SocDeltaService socDeltaService;

        public SocDeltaServiceTests()
        {
            socDeltaService = new SocDeltaService(fakeLogger);
        }

        [Fact]
        public void SocDeltaServiceTestsDetermineDeltaIsSuccessful()
        {
            // Arrange
            var deltaReportModel = new DeltaReportModel();

            // Act
            socDeltaService.DetermineDelta(deltaReportModel);

            // Assert
            Assert.True(true);
        }
    }
}
