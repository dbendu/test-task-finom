namespace ReportService.DataAccess.Models;

internal class DbEmployee
{
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;

    public string Inn { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;
}