using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ReportModels
{
    [ExcludeFromCodeCoverage]
    public class BreakdownModel
    {
        public string? Note { get; set; }

        public IList<BreakdownYearModel>? PredictedEmployment { get; set; }
    }
}
