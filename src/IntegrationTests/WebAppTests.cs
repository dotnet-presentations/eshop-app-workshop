using System.Net;
using IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace IntegrationTests;

public class WebAppTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task GetWebAppUrlsReturnsOkStatusCode()
    {
        var appHostBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.eShop_AppHost>();
        appHostBuilder.WriteOutputTo(outputHelper);
        appHostBuilder.WithRandomParameterValues();
        appHostBuilder.WithRandomVolumeNames();

        appHostBuilder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.ConfigureHttpClient(httpClient => httpClient.Timeout = Timeout.InfiniteTimeSpan);
            clientBuilder.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(2);
                options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(1);
                options.Retry.BackoffType = Polly.DelayBackoffType.Constant;
                options.Retry.Delay = TimeSpan.FromSeconds(5);
                options.Retry.MaxRetryAttempts = 30;
                options.CircuitBreaker.SamplingDuration = options.AttemptTimeout.Timeout * 2;
            });
        });

        await using var app = await appHostBuilder.BuildAsync();
        await app.StartAsync(waitForResourcesToStart: true);

        var httpClient = app.CreateHttpClient("webapp");

        var urls = new[] { "/alive", "/health", "/" };
        foreach (var url in urls)
        {
            var response = await httpClient.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}