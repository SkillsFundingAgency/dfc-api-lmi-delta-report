using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Contracts
{
    public interface ISocDeltaService
    {
        void DetermineDelta(DeltaReportModel? deltaReportModel);
    }
}
