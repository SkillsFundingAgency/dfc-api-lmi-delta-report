using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ReportModels
{
    [ExcludeFromCodeCoverage]
    public class JobProfileModel
    {
        public string? CanonicalName { get; set; }

        public string? Title { get; set; }
    }
}
