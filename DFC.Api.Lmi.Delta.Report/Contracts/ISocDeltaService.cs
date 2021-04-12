using DFC.Api.Lmi.Delta.Report.Models.ReportModels;

namespace DFC.Api.Lmi.Delta.Report.Contracts
{
    public interface ISocDeltaService
    {
        void DetermineDelta(FullDeltaReportModel? fullDeltaReportModel);
    }
}
