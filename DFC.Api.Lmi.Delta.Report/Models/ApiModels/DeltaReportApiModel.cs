using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ReportModels
{
    [ExcludeFromCodeCoverage]
    public class DeltaReportApiModel
    {
        public Guid? Ide { get; set; }

        public DateTime? CreatedDate { get; set; }

        public List<DeltaReportSocApiModel>? DeltaReportSocs { get; set; }

        public int SocDeltaCount { get; set; }
    }
}
