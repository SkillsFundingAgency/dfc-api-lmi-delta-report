using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.ReportModels
{
    [ExcludeFromCodeCoverage]
    public class BreakdownYearModel
    {
        public int Year { get; set; }

        public IList<BreakdownYearValueModel>? Breakdown { get; set; }
    }
}
