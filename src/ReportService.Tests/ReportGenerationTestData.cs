using System.Collections;
using Domain.Models;

namespace ReportService.Tests;

public class ReportGenerationTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        {
            var company = new CompanyStructure(
                [
                    new Department(
                        "Department 1",
                        [
                            new Employee(
                                1,
                                "Employee 1",
                                "1"
                            ),
                            new Employee(
                                2,
                                "Employee 2",
                                "2"
                            )
                        ]
                    ),
                    new Department(
                        "Department 2",
                        [
                            new Employee(
                                3,
                                "Employee 3",
                                "3"
                            ),
                            new Employee(
                                4,
                                "Employee 4",
                                "4"
                            )
                        ]
                    ),
                ]
            );

            var buhCodesMap = new Dictionary<string, string>
            {
                { "1", "buh code 1" },
                { "2", "buh code 2" },
                { "3", "buh code 3" },
                { "4", "buh code 4" }
            };

            // inn code - buh code - salary
            var salariesMap = new[]
            {
                ("1", "buh code 1", 100),
                ("2", "buh code 2", 200),
                ("3", "buh code 3", 300),
                ("4", "buh code 4", 400)
            };
            
            var expectedReport =
"""
**январь 1**

---

### Department 1

| Name       | Salary |
|------------|--------|
| Employee 1 | 100 €  |
| Employee 2 | 200 €  |

**Department Total: 300 €**

---

### Department 2

| Name       | Salary |
|------------|--------|
| Employee 3 | 300 €  |
| Employee 4 | 400 €  |

**Department Total: 700 €**

---

### Company Total: **1,000 €**

""";

            var month = 1;
            var year = 1;
            
            yield return
            [
                year,
                month,
                company,
                salariesMap,
                buhCodesMap,
                expectedReport
            ];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}