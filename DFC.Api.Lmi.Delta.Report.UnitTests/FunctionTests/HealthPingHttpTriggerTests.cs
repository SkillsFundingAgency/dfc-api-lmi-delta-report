using DFC.Api.Lmi.Delta.Report.Functions;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.FunctionTests
{
    public class HealthPingHttpTriggerTests
    {
        private readonly ILogger logger = A.Fake<ILogger>();

        [Fact]
        public void HealthPingHttpTriggerTestsReturnsOk()
        {
            // Arrange
            var context = new DefaultHttpContext();
            // Act
            var result = HealthPingHttpTrigger.Run(context.Request);

            // Assert
            Assert.IsType<OkResult>(result);
        }
    }
}
