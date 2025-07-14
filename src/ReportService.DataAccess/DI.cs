using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReportService.DataAccess.Settings;

namespace ReportService.DataAccess;

public static class Di
{
    public static IServiceCollection RegisterDataAccess(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<BuhServiceSettings>()
            .Bind(configuration.GetSection(nameof(BuhServiceSettings)))
            .ValidateDataAnnotations();

        services
            .AddOptions<SalaryServiceSettings>()
            .Bind(configuration.GetSection(nameof(SalaryServiceSettings)))
            .ValidateDataAnnotations();
        
        services
            .AddTransient<ICompanyStructureProvider, CompanyStructureProvider>()
            .AddTransient<IEmpCodeResolver, EmpCodeResolver>()
            .AddTransient<IReportSaver, ReportSaver>()
            .AddTransient<ISalariesProvider, SalariesProvider>();

        return services;
    }
}