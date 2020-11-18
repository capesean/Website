using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WEB.Models;
using Microsoft.Extensions.Options;
using System.Transactions;

namespace WEB.Controllers
{
    [Route("api/[Controller]"), AuthorizeRoles(Roles.Administrator)]
    public class SettingsController : BaseApiController
    {
        public SettingsController(ApplicationDbContext db, UserManager<User> um, Settings settings)
            : base(db, um, settings) { }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var settings = await db.Settings
                .SingleOrDefaultAsync();

            if (settings == null) settings = new DbSettings();

            return Ok(settings);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] DbSettingsDTO settingsDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var settings = await db.Settings
                .SingleOrDefaultAsync();

            if (settings == null)
            {
                settings = new DbSettings();
                db.Entry(settings).State = EntityState.Added;
            }
            else
            {
                db.Entry(settings).State = EntityState.Modified;
            }

            ModelFactory.Hydrate(settings, settingsDTO);
            
            await db.SaveChangesAsync();

            return await Get();
        }

    }
}
