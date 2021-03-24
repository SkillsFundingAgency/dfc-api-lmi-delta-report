using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ReportModels
{
    [ExcludeFromCodeCoverage]
    public class DeltaReportSocModel
    {
        public int Soc { get; set; }

        public JobGroupModel? DraftJobGroup { get; set; }

        public JobGroupModel? PublishedJobGroup { get; set; }
    }
}
