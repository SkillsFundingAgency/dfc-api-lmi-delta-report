using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ReportModels
{
    [ExcludeFromCodeCoverage]
    public class FullDeltaReportModel
    {
        public Guid? Id { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<DeltaReportSocModel>? DeltaReportSocs { get; set; }

        public int SocDeltaCount { get; set; }
    }
}
