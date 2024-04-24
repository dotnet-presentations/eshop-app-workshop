using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace IntegrationTests;

public class WebAppTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task GetWebAppUrlsReturnsOkStatusCode()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.eShop_AppHost>();
        appHost.WithRandomParameterValues();
        appHost.WithRandomVolumeNames();
        appHost.WriteOutputTo(new XUnitTextWriter(outputHelper));
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            var timeout = TimeSpan.FromSeconds(360);
            clientBuilder.ConfigureHttpClient(httpClient => httpClient.Timeout = timeout);
            clientBuilder.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = timeout;
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(120);
                options.Retry.BackoffType = Polly.DelayBackoffType.Constant;
                options.Retry.Delay = TimeSpan.FromSeconds(5);
                options.Retry.MaxRetryAttempts = 100;
                options.CircuitBreaker.SamplingDuration = options.AttemptTimeout.Timeout * 2;
            });
        });

        await using var app = await appHost.BuildAsync();
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