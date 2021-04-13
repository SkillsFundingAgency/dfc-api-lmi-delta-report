using DFC.Api.Lmi.Delta.Report.Contracts;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Connectors
{
    public class ApiDataConnector : IApiDataConnector
    {
        private readonly IApiConnector apiConnector;

        public ApiDataConnector(IApiConnector apiConnector)
        {
            this.apiConnector = apiConnector;
        }

        public async Task<TApiModel?> GetAsync<TApiModel>(HttpClient? httpClient, Uri url)
            where TApiModel : class
        {
            _ = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            var response = await apiConnector.GetAsync(httpClient, url, MediaTypeNames.Application.Json).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(response))
            {
                return JsonConvert.DeserializeObject<TApiModel>(response);
            }

            return default;
        }
    }
}
