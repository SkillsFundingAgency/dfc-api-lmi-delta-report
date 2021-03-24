using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models
{
    [ExcludeFromCodeCoverage]
    public class JobGroupSummaryItemModel
    {
        public Guid? Id { get; set; }

        public int Soc { get; set; }

        public string? Title { get; set; }
    }
}