using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Contracts
{
    public interface IJobGroupApiConnector
    {
        Task<IList<JobGroupSummaryItemModel>?> GetSummaryAsync();

        Task<JobGroupModel?> GetDetailAsync(int soc);

        Task<JobGroupModel?> GetDetailAsync(Guid socId);
    }
}
