using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Models.ClientOptions;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Services
{
    public class SocDeltaService : ISocDeltaService
    {
        private readonly ILogger<SocDeltaService> logger;

        public SocDeltaService(ILogger<SocDeltaService> logger)
        {
            this.logger = logger;
        }

        public void DetermineDelta(DeltaReportModel deltaReportModel)
        {
            logger.LogInformation("Identifying delta for report");


        }
    }
}
