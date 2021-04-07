using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ApiModels
{
    [ExcludeFromCodeCoverage]
    public class DeltaReportSummaryApiModel
    {
        public Guid? Id { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int SocDeltaCount { get; set; }
    }
}
