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

        public int SocImportedCount { get; set; }

        public int SocUnchangedCount { get; set; }

        public int SocAdditionCount { get; set; }

        public int SocUpdateCount { get; set; }

        public int SocDeletionCount { get; set; }
    }
}
