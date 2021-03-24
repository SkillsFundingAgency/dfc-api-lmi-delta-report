using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ClientOptions
{
    [ExcludeFromCodeCoverage]
    public class PublishedJobGroupClientOptions : JobGroupClientOptions
    {
        public int MaxReportsKept { get; set; } = 5;
    }
}
