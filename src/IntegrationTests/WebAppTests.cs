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
            client.AddStandardResilienceHandler();
        });
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();
        var urls = new[] { "/alive", "/health", "/" };

        var httpClient = app.CreateHttpClient("webapp");
        foreach (var url in urls)
        {
            var response = await httpClient.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}