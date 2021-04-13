using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ReportModels
{
    [ExcludeFromCodeCoverage]
    public class DeltaReportSocApiModel
    {
        public int Soc { get; set; }

        public string? DraftJobGroup { get; set; }

        public string? PublishedJobGroup { get; set; }

        public string? Delta { get; set; }
    }
}
