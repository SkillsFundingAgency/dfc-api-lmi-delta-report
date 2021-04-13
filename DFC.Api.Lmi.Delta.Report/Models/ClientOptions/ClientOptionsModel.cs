using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DFC.Api.Lmi.Delta.Report.Models.ClientOptions
{
    [ExcludeFromCodeCoverage]
    public abstract class ClientOptionsModel
    {
        public Uri? BaseAddress { get; set; }

        public TimeSpan Timeout { get; set; } = new TimeSpan(0, 0, 20);         // default to 20 seconds

        public string? ApiKey { get; set; }
    }
}
