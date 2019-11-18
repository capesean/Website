using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEB.Controllers;
using WEB.Models;
using WEB.Reports.PDF;

namespace KPI.Controllers
{
    [Route("[Controller]")]
    public class DownloadsController : BaseMvcController
    {
        public DownloadsController(ApplicationDbContext _db, UserManager<User> um, Settings _settings) : base(_db, um, _settings) { }

        //[AuthorizeRoles(Roles.Reports)]
        [HttpGet("test")]
        public async Task<IActionResult> ComparisonReport()
        {
            var report = new TestReport(db, Settings);

            byte[] bytes = await report.GenerateAsync();

            Response.Headers.Add("Content-Disposition", report.GetContentDisposition().ToString());

            return File(bytes, report.GetContentType());
        }
    }
}