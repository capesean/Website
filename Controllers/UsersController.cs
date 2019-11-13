using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WEB.Models;
using Microsoft.Extensions.Options;

namespace WEB.Controllers
{
    [Route("api/[Controller]"), Authorize]
    public class UsersController : BaseApiController
    {
        private RoleManager<AppRole> rm;
        private IOptions<PasswordOptions> opts;
        public UsersController(ApplicationDbContext _db, UserManager<User> _um, RoleManager<AppRole> _rm, IOptions<PasswordOptions> _opts) 
            : base(_db, _um) { rm = _rm; opts = _opts; }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery]PagingOptions pagingOptions, [FromQuery]string q = null, Guid? roleId = null)
        {
            if (pagingOptions == null) pagingOptions = new PagingOptions();

            IQueryable<User> results = userManager.Users;
            results = results.Include(o => o.Roles);

            if (roleId != null) results = results.Where(o => o.Roles.Any(r => r.RoleId == roleId));


            if (!string.IsNullOrWhiteSpace(q))
                results = results.Where(o => o.FirstName.Contains(q) || o.LastName.Contains(q));

            results = results.OrderBy(o => o.Id);

            var roles = await db.Roles.ToListAsync();

            return Ok((await GetPaginatedResponse(results, pagingOptions)).Select(o => ModelFactory.Create(o, roles)));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var user = await userManager.Users
                .Include(o => o.Roles)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (user == null)
                return NotFound();

            var roles = await db.Roles.ToListAsync();

            return Ok(ModelFactory.Create(user, roles));
        }

        [HttpPost("{id:Guid}"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> Save(Guid id, [FromBody]UserDTO userDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (userDTO.Id != id) return BadRequest("Id mismatch");

            var password = string.Empty;
            if (await db.Users.AnyAsync(o => o.Email == userDTO.Email && o.Id != userDTO.Id))
                return BadRequest("Email already exists.");

            var isNew = userDTO.Id == Guid.Empty;

            User user;
            if (isNew)
            {
                user = new User();
                password = Utilities.General.GenerateRandomPassword(opts.Value);

                db.Entry(user).State = EntityState.Added;
            }
            else
            {
                user = await userManager.Users
                    .Include(o => o.Roles)
                    .FirstOrDefaultAsync(o => o.Id == userDTO.Id);

                if (user == null)
                    return NotFound();

                db.Entry(user).State = EntityState.Modified;
            }

            ModelFactory.Hydrate(user, userDTO);

            var saveResult = (isNew ? await userManager.CreateAsync(user, password) : await userManager.UpdateAsync(user));

            if (!saveResult.Succeeded)
                return GetErrorResult(saveResult);

            var appRoles = await rm.Roles.ToListAsync();

            if (!isNew)
            {
                foreach (var roleId in user.Roles.ToList())
                {
                    var role = rm.Roles.Single(o => o.Id == roleId.RoleId);
                    await userManager.RemoveFromRoleAsync(user, role.Name);
                }
            }

            if (userDTO.Roles != null)
            {
                foreach (var roleName in userDTO.Roles)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
            }

            if (isNew) Utilities.General.SendWelcomeMail(user, password);

            return await Get(user.Id);
        }

        [HttpDelete("{id:Guid}"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(o => o.Id == id);

            if (user == null)
                return NotFound();

            await userManager.DeleteAsync(user);

            return Ok();
        }

    }
}
