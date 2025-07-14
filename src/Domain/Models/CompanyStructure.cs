namespace Domain.Models;

public record CompanyStructure(
    IReadOnlyCollection<Department> Departments
);

public record Department(
    string Name,
    IReadOnlyCollection<Employee> Employees
);

public record Employee(
    int Id,
    string Name,
    string Inn
);