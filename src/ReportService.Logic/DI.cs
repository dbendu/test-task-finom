using Microsoft.Extensions.DependencyInjection;

namespace ReportService.Logic;

public static class D
{
    public static IServiceCollection RegisterLogic(this IServiceCollection services)
    {
        services
            .AddTransient<IReportOrchestrator, ReportOrchestrator>()
            .AddTransient<IReportGenerator, ReportGenerator>();

        return services;
    }
}