using DFC.Api.Lmi.Delta.Report.Functions;
using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Compui.Subscriptions.Pkg.NetStandard.Data.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Xunit;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.FunctionTests
{
    [Trait("Category", "SubscriptionRegistration - Http trigger tests")]
    public class SubscriptionRegistrationHttpTriggerTests
    {
        private readonly ILogger<SubscriptionRegistrationHttpTrigger> fakeLogger = A.Fake<ILogger<SubscriptionRegistrationHttpTrigger>>();
        private readonly EnvironmentValues environmentValues = new EnvironmentValues();
        private readonly ISubscriptionRegistrationService fakeSubscriptionRegistrationService = A.Fake<ISubscriptionRegistrationService>();

        [Fact]
        public async Task SubscriptionRegistrationPostReturnsOk()
        {
            // Arrange
            var function = new SubscriptionRegistrationHttpTrigger(fakeLogger, environmentValues, fakeSubscriptionRegistrationService);

            // Act
            var result = await function.Run(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeSubscriptionRegistrationService.RegisterSubscription(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SubscriptionRegistrationPostThrowsException()
        {
            // Arrange
            A.CallTo(() => fakeSubscriptionRegistrationService.RegisterSubscription(A<string>.Ignored)).ThrowsAsync(new HttpRequestException());
            var function = new SubscriptionRegistrationHttpTrigger(fakeLogger, environmentValues, fakeSubscriptionRegistrationService);

            // Act
            var result = await function.Run(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeSubscriptionRegistrationService.RegisterSubscription(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.IsType<InternalServerErrorResult>(result);
        }
    }
}
