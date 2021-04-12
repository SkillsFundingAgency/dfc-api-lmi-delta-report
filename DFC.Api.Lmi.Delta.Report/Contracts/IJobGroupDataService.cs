using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using System;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Contracts
{
    public interface IJobGroupDataService
    {
        Task<FullDeltaReportModel> GetAllAsync();

        Task<FullDeltaReportModel?> GetSocAsync(Guid? socId);
    }
}
