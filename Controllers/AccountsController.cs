using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WEB.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using System.Net;

namespace WEB.Controllers
{
    [Route("api/[Controller]")]
    public class AccountsController : ControllerBase
    {
        private ApplicationDbContext db;
        private UserManager<User> userManager;
        private IEmailSender emailSender;
        private Settings settings;

        public AccountsController(ApplicationDbContext _db, UserManager<User> _userManager, IEmailSender _emailSender, IOptions<Settings> _settings)
        {
            db = _db;
            userManager = _userManager;
            emailSender = _emailSender;
            settings = _settings.Value;
        }

        [HttpPost("[Action]"), AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordDTO resetPasswordDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await db.Users.FirstOrDefaultAsync(o => o.UserName == resetPasswordDTO.UserName);
            if (user == null) return NotFound();

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var body = user.FirstName + Environment.NewLine;
            body += Environment.NewLine;
            body += "A password reset has been requested. Please use the link below to reset your password." + Environment.NewLine;
            body += Environment.NewLine;
            body += settings.RootUrl + "auth/reset?e=" + user.Email + "&t=" + WebUtility.UrlEncode(token) + Environment.NewLine;

            await emailSender.SendEmailAsync(user.Email, "Password Reset", body);

            return Ok();
        }

        [HttpPost("[Action]"), AllowAnonymous]
        public async Task<IActionResult> Reset([FromBody]ResetDTO resetDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (resetDTO.NewPassword != resetDTO.ConfirmPassword) return BadRequest("Passwords do not match");

            var user = await db.Users.FirstOrDefaultAsync(o => o.UserName == resetDTO.UserName);
            if (user == null) return NotFound(); // todo: should be BadRequest("Invalid email")?

            var result = await userManager.ResetPasswordAsync(user, resetDTO.Token, resetDTO.NewPassword);

            if (!result.Succeeded) return BadRequest(result.Errors.First().Description);

            var body = user.FirstName + Environment.NewLine;
            body += Environment.NewLine;
            body += "Your password has been reset." + Environment.NewLine;

            await emailSender.SendEmailAsync(user.Email, "Password Reset", body);

            return Ok();
        }

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

        public class ResetPasswordDTO
        {
            [Required]
            public string UserName { get; set; }
        }

        public class ResetDTO
        {
            [Required]
            public string UserName { get; set; }
            [Required]
            public string NewPassword { get; set; }
            [Required]
            public string ConfirmPassword { get; set; }
            [Required]
            public string Token { get; set; }
        }
    }
}
