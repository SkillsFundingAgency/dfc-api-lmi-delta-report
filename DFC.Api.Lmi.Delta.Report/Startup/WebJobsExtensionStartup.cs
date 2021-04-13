using AutoMapper;
using AzureFunctions.Extensions.Swashbuckle;
using DFC.Api.Lmi.Delta.Report.Connectors;
using DFC.Api.Lmi.Delta.Report.Contracts;
using DFC.Api.Lmi.Delta.Report.Extensions;
using DFC.Api.Lmi.Delta.Report.HttpClientPolicies;
using DFC.Api.Lmi.Delta.Report.Models;
using DFC.Api.Lmi.Delta.Report.Models.ClientOptions;
using DFC.Api.Lmi.Delta.Report.Models.ReportModels;
using DFC.Api.Lmi.Delta.Report.Services;
using DFC.Api.Lmi.Delta.Report.Startup;
using DFC.Compui.Cosmos;
using DFC.Compui.Cosmos.Contracts;
using DFC.Compui.Subscriptions.Pkg.Netstandard.Extensions;
using DFC.Swagger.Standard;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

[assembly: WebJobsStartup(typeof(WebJobsExtensionStartup), "Web Jobs Extension Startup")]

namespace DFC.Api.Lmi.Delta.Report.Startup
{
    [ExcludeFromCodeCoverage]
    public class WebJobsExtensionStartup : IWebJobsStartup
    {
        private const string AppSettingsPolicies = "Policies";
        private const string CosmosDbLmiDeltaReportConfigAppSettings = "Configuration:CosmosDbConnections:LmiDeltaReports";
        private const string CosmosDbLmiDeltaReportSocConfigAppSettings = "Configuration:CosmosDbConnections:LmiDeltaReportSocs";

        public void Configure(IWebJobsBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var cosmosDbDeltaReportConnection = configuration.GetSection(CosmosDbLmiDeltaReportConfigAppSettings).Get<CosmosDbConnection>();
            var cosmosDbDeltaReportSocConnection = configuration.GetSection(CosmosDbLmiDeltaReportSocConfigAppSettings).Get<CosmosDbConnection>();

            builder.Services.AddSingleton(configuration.GetSection(nameof(EventGridClientOptions)).Get<EventGridClientOptions>() ?? new EventGridClientOptions());

            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
            builder.Services.AddHttpClient();
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddAutoMapper(typeof(WebJobsExtensionStartup).Assembly);
            builder.Services.AddDocumentServices<DeltaReportModel>(cosmosDbDeltaReportConnection, false);
            builder.Services.AddDocumentServices<DeltaReportSocModel>(cosmosDbDeltaReportSocConnection, false);
            builder.Services.AddSubscriptionService(configuration);
            builder.Services.AddSingleton(new EnvironmentValues());
            builder.Services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddTransient<ILmiWebhookReceiverService, LmiWebhookReceiverService>();
            builder.Services.AddTransient<IEventGridService, EventGridService>();
            builder.Services.AddTransient<IEventGridClientService, EventGridClientService>();
            builder.Services.AddTransient<IApiConnector, ApiConnector>();
            builder.Services.AddTransient<IApiDataConnector, ApiDataConnector>();
            builder.Services.AddTransient<IJobGroupDataService, JobGroupDataService>();
            builder.Services.AddTransient<ISocDeltaService, SocDeltaService>();

            var policyOptions = configuration.GetSection(AppSettingsPolicies).Get<PolicyOptions>() ?? new PolicyOptions();
            var policyRegistry = builder.Services.AddPolicyRegistry();

            builder.Services.AddSingleton(configuration.GetSection(nameof(DraftJobGroupClientOptions)).Get<DraftJobGroupClientOptions>() ?? new DraftJobGroupClientOptions());
            builder.Services.AddSingleton(configuration.GetSection(nameof(PublishedJobGroupClientOptions)).Get<PublishedJobGroupClientOptions>() ?? new PublishedJobGroupClientOptions());

            builder.Services
                .AddPolicies(policyRegistry, nameof(DraftJobGroupClientOptions), policyOptions)
                .AddHttpClient<IDraftJobGroupApiConnector, DraftJobGroupApiConnector, DraftJobGroupClientOptions>(nameof(DraftJobGroupClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));
            builder.Services
                .AddPolicies(policyRegistry, nameof(PublishedJobGroupClientOptions), policyOptions)
                .AddHttpClient<IPublishedJobGroupApiConnector, PublishedJobGroupApiConnector, PublishedJobGroupClientOptions>(nameof(PublishedJobGroupClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));
        }
    }
}
