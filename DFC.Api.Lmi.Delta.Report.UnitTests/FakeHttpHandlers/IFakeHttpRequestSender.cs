using System.Net.Http;

namespace DFC.Api.Lmi.Delta.Report.UnitTests.FakeHttpHandlers
{
    public interface IFakeHttpRequestSender
    {
        HttpResponseMessage Send(HttpRequestMessage request);
    }
}
