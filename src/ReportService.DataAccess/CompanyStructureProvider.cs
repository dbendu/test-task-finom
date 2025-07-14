using Dapper;
using Domain;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ReportService.DataAccess.Models;

namespace ReportService.DataAccess;

public interface ICompanyStructureProvider
{
    Task<CompanyStructure> Get(CancellationToken token);
}

internal class CompanyStructureProvider : ICompanyStructureProvider
{
    private readonly IConfiguration _configuration;

    public CompanyStructureProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<CompanyStructure> Get(CancellationToken token)
    {
        var connectionString = _configuration.GetConnectionString("EmployeesDb");
        
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(token);

        var command = new CommandDefinition(
            """
            select
                emps.id as Id,
                emps.name as Name,
                emps.inn as Inn,
                deps.name as DepartmentName
            from emps
            join deps on emps.departmentid = deps.id
            """,
            cancellationToken: token
        );

        var employees = await connection.QueryAsync<DbEmployee>(command);

        var departments = employees
            .GroupBy(employee => employee.DepartmentName)
            .Select(group => new Department(
                group.Key,
                group
                    .Select(employee => new Employee(
                        employee.Id,
                        employee.Name,
                        employee.Inn))
                    .ToArray()))
            .ToArray();

        return new CompanyStructure(departments);
    }
}