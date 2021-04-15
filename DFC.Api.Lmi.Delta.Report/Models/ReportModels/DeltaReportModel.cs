using DFC.Compui.Cosmos.Contracts;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DFC.Api.Lmi.Delta.Report.Models.ReportModels
{
    [ExcludeFromCodeCoverage]
    public class DeltaReportModel : DocumentModel
    {
        public override string? PartitionKey
        {
            get => CreatedDate.ToString("O", CultureInfo.InvariantCulture);

            set => CreatedDate = DateTime.Parse(value ?? DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int SocImportedCount { get; set; }

        public int SocUnchangedCount { get; set; }

        public int SocAdditionCount { get; set; }

        public int SocUpdateCount { get; set; }

        public int SocDeletionCount { get; set; }
    }
}
