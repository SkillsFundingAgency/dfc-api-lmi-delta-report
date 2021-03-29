using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using System;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Contracts
{
    public interface IJobGroupDataService
    {
        Task<DeltaReportModel> GetAll();

        Task<DeltaReportModel?> GetSoc(Guid? socId);
    }
}
