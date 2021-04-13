using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ClientOptions
{
    [ExcludeFromCodeCoverage]
    public abstract class JobGroupClientOptions : ClientOptionsModel
    {
        public string SummaryEndpoint { get; set; } = "summary";

        public string DetailEndpoint { get; set; } = "detail";
    }
}
