using Domain.Models;
using Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ReportService.DataAccess;
using ReportService.Logic;
using Xunit;

namespace ReportService.Tests;

public class ReportGenerationTest
{
    [Theory]
    [ClassData(typeof(ReportGenerationTestData))]
    public async Task Test(
        int year,
        int month,
        CompanyStructure company,
        IReadOnlyCollection<(string Inn, string BuhCode, int Salary)> salariesMap,
        IReadOnlyDictionary<string, string> buhCodesMap,
        string expectedReport)
    {
        var companyDataProviderMock = new Mock<ICompanyStructureProvider>();
        
        companyDataProviderMock
            .Setup(it => it.Get(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(company));

        var salariesProviderMock = new Mock<ISalariesProvider>();

        foreach (var salaryMapping in salariesMap)
            salariesProviderMock
                .Setup(it => it.GetSalary(salaryMapping.Inn, salaryMapping.BuhCode, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(salaryMapping.Salary));

        var parallelismSettingsMock = new Mock<IOptionsMonitor<ParallelismSettings>>();

        parallelismSettingsMock
            .Setup(it => it.CurrentValue)
            .Returns(new ParallelismSettings
                {
                    MaxParallelTasksCount = 1
                }
            );

        var buhCodeResolverMock = new Mock<IEmpCodeResolver>();

        foreach (var buhCodeMapping in buhCodesMap)
            buhCodeResolverMock
                .Setup(it => it.Resolve(buhCodeMapping.Key, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(buhCodeMapping.Value));

        var reportGenerator = new ReportGenerator(
            salariesProviderMock.Object,
            parallelismSettingsMock.Object,
            buhCodeResolverMock.Object
        );

        var reportSaverMock = new Mock<IReportSaver>();

        reportSaverMock
            .Setup(it => it.Save(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var orchestrator = new ReportOrchestrator(
            companyDataProviderMock.Object,
            reportGenerator,
            reportSaverMock.Object,
            new Mock<ILogger<IReportOrchestrator>>().Object
        );

        var report = await orchestrator.CreateReport(year, month, CancellationToken.None);

        Assert.Equal(expectedReport, report);
    }
}