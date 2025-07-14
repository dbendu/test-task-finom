using Microsoft.Extensions.Options;
using ReportService.DataAccess.Settings;

namespace ReportService.DataAccess;

public interface IEmpCodeResolver
{
    Task<string> Resolve(string inn, CancellationToken token);
}

internal class EmpCodeResolver : IEmpCodeResolver
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BuhServiceSettings _serviceSettings;

    public EmpCodeResolver(
        IHttpClientFactory httpClientFactory,
        IOptions<BuhServiceSettings> serviceSettings
    )
    {
        _httpClientFactory = httpClientFactory;
        _serviceSettings = serviceSettings.Value;
    }

    public async Task<string> Resolve(string inn, CancellationToken token)
    {
        var client = _httpClientFactory.CreateClient();

        client.BaseAddress = _serviceSettings.Url;
        client.Timeout = _serviceSettings.Timeout;

        var response = await client.GetStringAsync("api/inn/" + inn, token);

        return response;
    }
}