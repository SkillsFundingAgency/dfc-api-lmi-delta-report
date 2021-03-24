using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Api.Lmi.Delta.Report.Models.ClientOptions;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Delta.Report.Contracts
{
    public interface IEventGridService
    {
        Task SendEventAsync(EventGridEventData? eventGridEventData, string? subject, string? eventType);

        bool IsValidEventGridClientOptions(EventGridClientOptions? eventGridClientOptions);
    }
}
