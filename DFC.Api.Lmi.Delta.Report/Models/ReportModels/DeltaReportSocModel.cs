using DFC.Api.Lmi.Delta.Report.Common;
using DFC.Compui.Cosmos.Contracts;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ReportModels
{
    [ExcludeFromCodeCoverage]
    public class DeltaReportSocModel : DocumentModel
    {
        public override string? PartitionKey
        {
            get => DeltaReportId.ToString();

            set
            {
                if (value == null)
                {
                    DeltaReportId = null;
                }
                else
                {
                    DeltaReportId = Guid.Parse(value);
                }
            }
        }

        public Guid? DeltaReportId { get; set; }

        public int Soc { get; set; }

        public string? SocTitle { get; set; }

        public DeltaReportState State { get; set; }

        public string? Delta { get; set; }

        public JobGroupModel? DraftJobGroup { get; set; }

        public JobGroupModel? PublishedJobGroup { get; set; }
    }
}
