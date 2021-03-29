using DFC.Api.Lmi.Delta.Report.HttpClientPolicies;
using DFC.Api.Lmi.Delta.Report.Models.ClientOptions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace DFC.Api.Lmi.Delta.Report.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPolicies(
            this IServiceCollection services,
            IPolicyRegistry<string> policyRegistry,
            string keyPrefix,
            PolicyOptions policyOptions)
        {
            _ = policyOptions ?? throw new ArgumentNullException(nameof(policyOptions));

            policyRegistry?.Add(
                $"{keyPrefix}_{nameof(PolicyOptions.HttpRetry)}",
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    .OrResult(r => r?.Headers?.RetryAfter != null)
                    .WaitAndRetryAsync(
                        policyOptions.HttpRetry.Count,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(policyOptions.HttpRetry.BackoffPower, retryAttempt))));

            policyRegistry?.Add(
                $"{keyPrefix}_{nameof(PolicyOptions.HttpCircuitBreaker)}",
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: policyOptions.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                        durationOfBreak: policyOptions.HttpCircuitBreaker.DurationOfBreak));

            return services;
        }

        public static IServiceCollection AddHttpClient<TClient, TImplementation, TClientOptions>(
            this IServiceCollection services,
            string configurationSectionName,
            string retryPolicyName,
            string circuitBreakerPolicyName)
            where TClient : class
            where TImplementation : class, TClient
            where TClientOptions : ClientOptionsModel, new()
        {
            return services
                    .AddHttpClient<TClient, TImplementation>()
                    .ConfigureHttpClient((sp, options) =>
                    {
                        var httpClientOptions = sp.GetRequiredService<TClientOptions>();
                        options.BaseAddress = httpClientOptions.BaseAddress;
                        options.Timeout = httpClientOptions.Timeout;

                        if (!string.IsNullOrWhiteSpace(httpClientOptions.ApiKey))
                        {
                            options.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", httpClientOptions.ApiKey);
                        }
                    })
                    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                    {
                        AllowAutoRedirect = false,
                    })
                    .AddPolicyHandlerFromRegistry($"{configurationSectionName}_{retryPolicyName}")
                    .AddPolicyHandlerFromRegistry($"{configurationSectionName}_{circuitBreakerPolicyName}")
                    .Services;
        }
    }
}
