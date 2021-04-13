using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Delta.Report.Models.FunctionRequestModels
{
    [ExcludeFromCodeCoverage]
    public class EventGridPostRequestModel
    {
        public Guid? ItemId { get; set; }

        public string? Api { get; set; }

        public string? DisplayText { get; set; }

        public string? EventType { get; set; }
    }
}
