using Domain.Settings;
using Microsoft.Extensions.Options;

namespace ReportService.DataAccess;

public interface IReportSaver
{
    Task Save(string report, CancellationToken token);
}

internal class ReportSaver : IReportSaver
{
    private readonly ReportSettings _settings;

    public ReportSaver(IOptionsMonitor<ReportSettings> settings)
    {
        _settings = settings.CurrentValue;
    }

    public async Task Save(string report, CancellationToken token)
    {
        var dir = Path.GetDirectoryName(_settings.ReportPath);

        if (dir is null)
            throw new ArgumentNullException(nameof(dir), "Directory is expected not to be null");

        Directory.CreateDirectory(dir);
        
        await File.WriteAllTextAsync(_settings.ReportPath, report, token);
    }
}