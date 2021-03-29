using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.HttpClientPolicies
{
    [ExcludeFromCodeCoverage]
    public class RetryPolicyOptions
    {
        public int Count { get; set; } = 3;

        public int BackoffPower { get; set; } = 2;
    }
}
