using System.ComponentModel.DataAnnotations;

namespace ReportService.DataAccess.Settings;

internal class SalaryServiceSettings
{
    [Required]
    public Uri Url { get; set; } = null!;
    
    [Required]
    public TimeSpan Timeout { get; set; }
}