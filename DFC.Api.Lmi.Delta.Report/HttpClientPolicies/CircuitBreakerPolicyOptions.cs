using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.HttpClientPolicies
{
    [ExcludeFromCodeCoverage]
    public class CircuitBreakerPolicyOptions
    {
        public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);

        public int ExceptionsAllowedBeforeBreaking { get; set; } = 3;
    }
}
