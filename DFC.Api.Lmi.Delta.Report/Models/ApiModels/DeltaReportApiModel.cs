using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ReportModels
{
    [ExcludeFromCodeCoverage]
    public class DeltaReportApiModel
    {
        public Guid? Id { get; set; }

        public DateTime? CreatedDate { get; set; }

        public List<DeltaReportSocApiModel>? DeltaReportSocs { get; set; }

        public int SocImportedCount { get; set; }

        public int SocUnchangedCount { get; set; }

        public int SocAdditionCount { get; set; }

        public int SocUpdateCount { get; set; }

        public int SocDeletionCount { get; set; }
    }
}
