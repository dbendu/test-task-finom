using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using ReportService.Logic;

namespace ReportService.Api.Controllers
{
    [Route("api/[controller]")]
    public class ReportController : Controller
    {
        private readonly IReportOrchestrator _orchestrator;

        public ReportController(IReportOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet]
        [Route("{year}/{month}")]
        public async Task<IActionResult> Download(
            [Required] [FromRoute] int year,
            [Required] [FromRoute] int month,
            CancellationToken token)
        {
            var report = await _orchestrator.CreateReport(year, month, token);

            return File(
                Encoding.UTF8.GetBytes(report),
                "application/octet-stream",
                "report.txt"
            );
        }
    }
}
