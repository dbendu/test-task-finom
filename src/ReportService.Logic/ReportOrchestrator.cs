using Microsoft.Extensions.Logging;
using ReportService.DataAccess;

namespace ReportService.Logic;

public interface IReportOrchestrator
{
    Task<string> CreateReport(
        int year,
        int month,
        CancellationToken token
    );
}

internal class ReportOrchestrator : IReportOrchestrator
{
    private readonly ICompanyStructureProvider _dataProvider;
    private readonly IReportGenerator _reportGenerator;
    private readonly IReportSaver _reportSaver;
    private readonly ILogger<IReportOrchestrator> _logger;

    public ReportOrchestrator(
        ICompanyStructureProvider dataProvider,
        IReportGenerator reportGenerator, 
        IReportSaver reportSaver,
        ILogger<IReportOrchestrator> logger)
    {
        _dataProvider = dataProvider;
        _reportGenerator = reportGenerator;
        _reportSaver = reportSaver;
        _logger = logger;
    }

    public async Task<string> CreateReport(
        int year,
        int month,
        CancellationToken token
    )
    {
        var company = await _dataProvider.Get(token);

        var report = await _reportGenerator.Generate(year, month, company, token);

        try
        {
            await _reportSaver.Save(report, token);
        }
        catch (Exception ex)
        {
            // исключение при сохранении не мешает нам продолжить работу с отчётом, поэтому не пробрасываем его выше
            _logger.LogError(ex, "Can't save report file");
        }

        return report;
    }
}