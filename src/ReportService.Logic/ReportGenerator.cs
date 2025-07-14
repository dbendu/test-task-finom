using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Domain.Models;
using Domain.Settings;
using Microsoft.Extensions.Options;
using ReportService.DataAccess;

namespace ReportService.Logic;

internal interface IReportGenerator
{
    Task<string> Generate(
        int year,
        int month,
        CompanyStructure company,
        CancellationToken token
    );
}

internal class ReportGenerator : IReportGenerator
{
    private readonly ISalariesProvider _salariesProvider;
    private readonly ParallelismSettings _parallelismSettings;
    private readonly IEmpCodeResolver _buhCodeResolver;

    public ReportGenerator(
        ISalariesProvider salariesProvider,
        IOptionsMonitor<ParallelismSettings> parallelismSettings,
        IEmpCodeResolver buhCodeResolver
    )
    {
        _salariesProvider = salariesProvider;
        _buhCodeResolver = buhCodeResolver;
        _parallelismSettings = parallelismSettings.CurrentValue;
    }

    public async Task<string> Generate(
        int year,
        int month,
        CompanyStructure company,
        CancellationToken token
    )
    {
        var salaries = await LoadSalaries(
            company.Departments
                .SelectMany(department => department.Employees)
                .ToArray(),
            token
        );

        return Generate(year, month, company.Departments, salaries);
    }

    /// <returns>Map {user id -- salary}</returns>
    private async Task<IReadOnlyDictionary<int, int>> LoadSalaries(
        IReadOnlyCollection<Employee> employees,
        CancellationToken token
    )
    {
        var parallelTasksCount = Math.Min(
            _parallelismSettings.MaxParallelTasksCount,
            Environment.ProcessorCount
        );

        var salariesMap = new ConcurrentDictionary<int, int>(
            concurrencyLevel: parallelTasksCount,
            capacity: employees.Count
        );

        await Parallel.ForEachAsync(
            employees,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = _parallelismSettings.MaxParallelTasksCount,
                CancellationToken = token
            },
            async (employee, ct) =>
            {
                var buhCode = await _buhCodeResolver.Resolve(employee.Inn, ct);
                
                var salary = await _salariesProvider.GetSalary(employee.Inn, buhCode, ct);

                salariesMap.TryAdd(employee.Id, salary);
            }
        );

        return salariesMap;
    }

    private string Generate(
        int year,
        int month,
        IReadOnlyCollection<Department> departments,
        IReadOnlyDictionary<int, int> salaries
    )
    {
        var builder = new StringBuilder(1000);

        var namesColumnWidth = CalculateNameColumnWidth();
        var salariesColumnWidth = CalculateSalariesColumnWidth();

        var totalSalariesCounter = 0;
        
        GenerateDateString();

        foreach (var department in departments)
        {
            AddDelimiter();
            
            var departmentTotal = department.Employees.Sum(employee => salaries[employee.Id]);
            
            totalSalariesCounter += departmentTotal;

            AddDepartmentName(department);
            
            AddGap();
            
            AddColumnsHeader();

            foreach (var employee in department.Employees)
            {
                AddEmployee(employee);
                builder.AppendLine();
            }

            builder.AppendLine();

            AddDepartmentTotal(departmentTotal);
        }
        
        AddDelimiter();

        AddCompanyTotal(totalSalariesCounter);

        builder.AppendLine();

        return builder.ToString();

        int CalculateNameColumnWidth()
        {
            var employees = departments
                .SelectMany(department => department.Employees)
                .ToArray();

            if (employees.Length is 0)
                return " Name ".Length;

            var maxNameLength = employees.Max(employee => employee.Name.Length);

            // учитываем пробелы в начале и в конце строки, чтобы имена не слипались с колонками
            var totalLength = maxNameLength + 2;

            return Math.Max(
                totalLength,
                " Name ".Length
            );
        }
        
        int CalculateSalariesColumnWidth()
        {
            if (salaries.Count is 0)
                return " Salary ".Length;

            var maxSalary = salaries
                .Select(pair => pair.Value)
                .Max();
            
            if (maxSalary == 0)
                return " Salary ".Length;

            int digits = (int)Math.Floor(Math.Log10(maxSalary)) + 1;
            
            int commas = (digits - 1) / 3;

            var totalLength = digits +
                              commas +
                              2 + // отделённый пробелом знак валюты
                              2; // учитываем пробелы в начале и в конце строки, чтобы имена не слипались с колонками

            return Math.Max(
                totalLength,
                " Salary ".Length
            );
        }

        void GenerateDateString()
        {
            builder
                .Append("**")
                .Append(MonthNameResolver.MonthName.GetName(year, month))
                .Append(' ')
                .Append(year)
                .Append("**");
        }

        void AddGap()
        {
            builder
                .AppendLine()
                .AppendLine();
        }

        void AddDelimiter()
        {
            AddGap();

            builder.Append("---");
                
            AddGap();
        }

        void AddDepartmentName(Department department)
        {
            builder
                .Append("### ")
                .Append(department.Name);
        }

        void AddColumnsHeader()
        {
            builder
                .Append("| Name")
                .Append(' ', namesColumnWidth - 5) // вычитаем длину предыдущей строки
                .Append("| Salary")
                .Append(' ', salariesColumnWidth - 7) // вычитаем длину предыдущей строки
                .Append('|')
                .AppendLine()

                .Append('|')
                .Append('-', namesColumnWidth)
                .Append('|')
                .Append('-', salariesColumnWidth)
                .Append('|')
                .AppendLine();
        }

        void AddEmployee(Employee employee)
        {
            var salary = FormatSalary(salaries[employee.Id]);

            builder
                .Append("| ")
                .Append(employee.Name)
                .Append(' ', namesColumnWidth - employee.Name.Length - 1) // вычитаем длину имени и пробела перед ним
                .Append("| ")
                .Append(salary)
                .Append(" €")
                .Append(' ', salariesColumnWidth - salary.Length - 3) // вычитаем длину зарплаты, знака валюты и финального пробела
                .Append('|');
        }

        void AddDepartmentTotal(int total)
        {
            var formattedTotal = FormatSalary(total);

            builder
                .Append("**Department Total: ")
                .Append(formattedTotal)
                .Append(" €**");
        }

        void AddCompanyTotal(int total)
        {
            var formattedTotal = FormatSalary(total);

            builder
                .Append("### Company Total: **")
                .Append(formattedTotal)
                .Append(" €**");
        }

        string FormatSalary(int salary) => salary.ToString("N0", CultureInfo.InvariantCulture);
    }
}