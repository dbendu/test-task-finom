using System.ComponentModel.DataAnnotations;

namespace Domain.Settings;

public class ReportSettings
{
    [Required]
    public string ReportPath { get; set; } = null!;
}