using DFC.Api.Lmi.Delta.Report.Models.FunctionRequestModels;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Contracts
{
    public interface ILmiWebhookReceiverService
    {
        WebhookRequestModel ExtractEvent(string requestBody);

        Task<HttpStatusCode> ReportAll();

        Task<HttpStatusCode> ReportSoc(Guid? socId);
    }
}
