using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ReportModels
{
    [ExcludeFromCodeCoverage]
    public class JobGrowthPredictionModel
    {
        public int StartYearRange { get; set; }

        public int EndYearRange { get; set; }

        public decimal JobsCreated { get; set; }

        public decimal PercentageGrowth { get; set; }

        public decimal? Retirements { get; set; }

        public decimal? PercentageRetirements { get; set; }
    }
}
