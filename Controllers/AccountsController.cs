using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WEB.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WEB.Controllers
{
    //todo: protect or remove this contoller?
    [Route("api/[Controller]")]
    public class AccountsController : BaseApiController
    {
        public AccountsController(ApplicationDbContext _db, UserManager<User> um) : base(_db, um) { }

        [HttpPost("[Action]"), AllowAnonymous]
        public async Task<IActionResult> Register([FromBody]RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new User
            {
                UserName = registerDTO.UserName,
                Email = registerDTO.UserName,
                FirstName = "first name",
                LastName = "last name",
                EmailConfirmed = true
            };

            var result = await this.userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded) return BadRequest(string.Join(", ", result.Errors.Select(o => o.Code + ": " + o.Description)));

            await userManager.AddToRoleAsync(user, Roles.Administrator.ToString());

            return Ok();
        }

        public class RegisterDTO
        {
            [Required]
            public string UserName { get; set; }
            [Required]
            public string Password { get; set; }
        }
    }
}
