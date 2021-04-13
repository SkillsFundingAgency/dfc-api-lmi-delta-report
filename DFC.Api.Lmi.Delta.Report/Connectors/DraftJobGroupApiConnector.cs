using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Models.ClientOptions;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace DFC.Api.Lmi.Delta.Report.Connectors
{
    public class DraftJobGroupApiConnector : JobGroupApiConnector, IDraftJobGroupApiConnector
    {
        public DraftJobGroupApiConnector(
        ILogger<DraftJobGroupApiConnector> logger,
        HttpClient httpClient,
        IApiDataConnector apiDataConnector,
        DraftJobGroupClientOptions draftJobGroupClientOptions) : base(logger, httpClient, apiDataConnector, draftJobGroupClientOptions)
        { }
    }
}
