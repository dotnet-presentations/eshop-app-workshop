using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class WebAppTests
{
    [Fact]
    public async Task GetWebAppUrlsReturnsOkStatusCode()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.eShop_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(client =>
        {
            client.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(360);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(120);
                options.Retry.BackoffType = Polly.DelayBackoffType.Constant;
                options.Retry.Delay = TimeSpan.FromSeconds(5);
                options.Retry.MaxRetryAttempts = 100;
                options.CircuitBreaker.SamplingDuration = options.AttemptTimeout.Timeout * 2;
            });
        });

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("webapp");

        var urls = new[] { "/alive", "/health", "/" };
        foreach (var url in urls)
        {
            var response = await httpClient.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}