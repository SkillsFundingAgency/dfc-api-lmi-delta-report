using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Models.ClientOptions;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace DFC.Api.Lmi.Delta.Report.Connectors
{
    public class PublishedJobGroupApiConnector : JobGroupApiConnector, IPublishedJobGroupApiConnector
    {
        public PublishedJobGroupApiConnector(
        ILogger<PublishedJobGroupApiConnector> logger,
        HttpClient httpClient,
        IApiDataConnector apiDataConnector,
        PublishedJobGroupClientOptions publishedJobGroupClientOptions) : base(logger, httpClient, apiDataConnector, publishedJobGroupClientOptions)
        { }
    }
}
