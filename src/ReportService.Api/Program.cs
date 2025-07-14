using Domain.Settings;
using ReportService.DataAccess;
using ReportService.Logic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient();

builder.Services
    .RegisterLogic()
    .RegisterDataAccess(builder.Configuration);

builder.Services
    .AddOptions<ParallelismSettings>()
    .Bind(builder.Configuration.GetSection(nameof(ParallelismSettings)))
    .ValidateDataAnnotations();

builder.Services
    .AddOptions<ReportSettings>()
    .Bind(builder.Configuration.GetSection(nameof(ReportSettings)))
    .ValidateDataAnnotations();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();