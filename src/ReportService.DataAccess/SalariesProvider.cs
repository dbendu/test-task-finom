using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportService.DataAccess.Settings;

namespace ReportService.DataAccess;

public interface ISalariesProvider
{
    Task<int> GetSalary(string inn, string buhCode, CancellationToken token);
}

internal class SalariesProvider : ISalariesProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SalaryServiceSettings _serviceSettings;

    public SalariesProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<SalaryServiceSettings> serviceSettings
    )
    {
        _httpClientFactory = httpClientFactory;
        _serviceSettings = serviceSettings.Value;
    }

    public async Task<int> GetSalary(
        string inn,
        string buhCode,
        CancellationToken token
    )
    {
        var client = _httpClientFactory.CreateClient();

        client.BaseAddress = _serviceSettings.Url;
        client.Timeout = _serviceSettings.Timeout;

        var payload = new
        {
            buhCode
        };

        var json = JsonSerializer.Serialize(payload);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("api/empcode/" + inn, content, token);

        var responsePayload = await response.Content.ReadAsStringAsync(token);

        if (response.StatusCode is not HttpStatusCode.OK)
        {
            throw new Exception($"Salary request finished with code: {response.StatusCode}, {responsePayload}");
        }

        return int.Parse(responsePayload);
    }
}