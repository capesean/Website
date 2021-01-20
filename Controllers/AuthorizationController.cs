using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Validation;
using WEB;
using WEB.Controllers;
using WEB.Models;
using WEB.Models.Authorization;
using OpenIddict.Server.AspNetCore;
using Microsoft.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthorizationServer.Controllers
{
    [Route("api/[Controller]")]
    public class AuthorizationController : BaseApiController
    {
        private readonly SignInManager<User> _signInManager;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private IEmailSender emailSender;

        public AuthorizationController(
            ApplicationDbContext _db,
            UserManager<User> _um,
            Settings _settings,
            SignInManager<User> _sm,
            IOptions<IdentityOptions> _id,
            IEmailSender _es)
            : base(_db, _um, _settings)
        {
            _signInManager = _sm;
            _identityOptions = _id;
            emailSender = _es;
        }

        [HttpPost("~/connect/token"), Produces("application/json"), AllowAnonymous]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.IsPasswordGrantType())
            {
                var user = await userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    return Forbid(
                       authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                       properties: new AuthenticationProperties(new Dictionary<string, string>
                       {
                           [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                           [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The login details are invalid."
                       }));
                }

                if (!user.Enabled || user.LockoutEnd > DateTime.UtcNow)
                {
                    return Forbid(
                       authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                       properties: new AuthenticationProperties(new Dictionary<string, string>
                       {
                           [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.AccessDenied,
                           [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user has been disabled."
                       }));
                }

                // Validate the username/password parameters and ensure the account is not locked out.
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    return Forbid(
                       authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                       properties: new AuthenticationProperties(new Dictionary<string, string>
                       {
                           [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                           [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The login details are invalid."
                       }));
                }

                var principal = await _signInManager.CreateUserPrincipalAsync(user);

                principal.SetScopes(new[]
                {
                    Scopes.OpenId,
                    Scopes.Email,
                    Scopes.Profile,
                    Scopes.OfflineAccess,
                    Scopes.Roles
                }.Intersect(request.GetScopes()));

                //user.LastLoginDateUTC = DateTime.UtcNow;
                //await userManager.UpdateAsync(user);

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(GetDestinations(claim, principal));
                }

                // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            else if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

                // Retrieve the user profile corresponding to the refresh token.
                // Note: if you want to automatically invalidate the refresh token
                // when the user password/roles change, use the following line instead:
                // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
                var user = await userManager.GetUserAsync(principal);
                if (user is null)
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                        }));
                }

                if (!user.Enabled || user.LockoutEnd > DateTime.UtcNow)
                {
                    return Forbid(
                       authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                       properties: new AuthenticationProperties(new Dictionary<string, string>
                       {
                           [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.AccessDenied,
                           [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user has been disabled."
                       }));
                }

                // Ensure the user is still allowed to sign in.
                if (!await _signInManager.CanSignInAsync(user))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                        }));
                }

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(GetDestinations(claim, principal));
                }

                // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        [HttpGet, Route("profile")]
        public async Task<IActionResult> Profile()
        {
            // add properties to profile as needed
            var roleIds = CurrentUser.Roles.Select(o => o.RoleId).ToArray();

            var roleNames = await db.Roles
                .Where(o => roleIds.Contains(o.Id))
                .Select(o => o.Name)
                .ToListAsync();

            var profile = new ProfileModel
            {
                Email = CurrentUser.Email,
                FirstName = CurrentUser.FirstName,
                LastName = CurrentUser.LastName,
                FullName = CurrentUser.FullName,
                UserId = CurrentUser.Id,
                Roles = roleNames,
                UserName = CurrentUser.UserName
            };

            return Ok(profile);
        }

        //private async Task<AuthenticationTicket> CreateTicketAsync(
        //    OpenIdConnectRequest request, User user,
        //    AuthenticationProperties properties = null)
        //{
        //    // Create a new ClaimsPrincipal containing the claims that
        //    // will be used to create an id_token, a token or a code.
        //    var principal = await _signInManager.CreateUserPrincipalAsync(user);

        //    // Create a new authentication ticket holding the user identity.
        //    var ticket = new AuthenticationTicket(principal, properties,
        //        OpenIddictServerDefaults.AuthenticationScheme);

        //    if (!request.IsRefreshTokenGrantType())
        //    {
        //        // Set the list of scopes granted to the client application.
        //        // Note: the offline_access scope must be granted
        //        // to allow OpenIddict to return a refresh token.
        //        ticket.SetScopes(new[]
        //        {
        //            OpenIdConnectConstants.Scopes.OpenId,
        //            OpenIdConnectConstants.Scopes.Email,
        //            OpenIdConnectConstants.Scopes.Profile,
        //            OpenIdConnectConstants.Scopes.OfflineAccess,
        //            OpenIddictConstants.Scopes.Roles
        //        }.Intersect(request.GetScopes()));
        //    }

        //    ticket.SetResources("resource_server");

        //    // Note: by default, claims are NOT automatically included in the access and identity tokens.
        //    // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        //    // whether they should be included in access tokens, in identity tokens or in both.

        //    foreach (var claim in ticket.Principal.Claims)
        //    {
        //        // Never include the security stamp in the access and identity tokens, as it's a secret value.
        //        if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
        //        {
        //            continue;
        //        }

        //        var destinations = new List<string>
        //        {
        //            OpenIdConnectConstants.Destinations.AccessToken
        //        };

        //        // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
        //        // The other claims will only be added to the access_token, which is encrypted when using the default format.
        //        if ((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
        //            (claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email)) ||
        //            (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
        //        {
        //            destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
        //        }

        //        claim.SetDestinations(destinations);
        //    }

        //    var identity = (ClaimsIdentity)principal.Identity;

        //    var firstNameClaim = new Claim("firstName", user.FirstName, ClaimValueTypes.String);
        //    firstNameClaim.SetDestinations(OpenIdConnectConstants.Destinations.AccessToken, OpenIdConnectConstants.Destinations.IdentityToken);
        //    identity.AddClaim(firstNameClaim);

        //    var lastNameClaim = new Claim("lastName", user.LastName, ClaimValueTypes.String);
        //    lastNameClaim.SetDestinations(OpenIdConnectConstants.Destinations.AccessToken, OpenIdConnectConstants.Destinations.IdentityToken);
        //    identity.AddClaim(lastNameClaim);

        //    return ticket;
        //}

        private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch (claim.Type)
            {
                case Claims.Name:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp": yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }

        [HttpPost("[Action]")]//, Authorize(AuthenticationSchemes = OpenIddictValidationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            // todo: check if enabled? user.enabled - also in login, reset, BaseApiController, etc.
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (changePasswordDTO.NewPassword != changePasswordDTO.ConfirmPassword) return BadRequest("Passwords do not match");

            var user = await db.Users.FirstOrDefaultAsync(o => o.UserName == User.Identity.Name);
            if (user == null) return NotFound();

            var result = await userManager.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);

            if (!result.Succeeded) return BadRequest(result.Errors.First().Description);

            var body = user.FirstName + Environment.NewLine;
            body += Environment.NewLine;
            body += "Your password has been changed." + Environment.NewLine;

            await emailSender.SendEmailAsync(user.Email, user.FullName, "Password Changed", body);

            return Ok();
        }

        [HttpPost("[Action]"), AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await db.Users.FirstOrDefaultAsync(o => o.UserName == resetPasswordDTO.UserName);
            if (user == null) return NotFound();

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var text = user.FirstName + Environment.NewLine;
            text += Environment.NewLine;
            text += "A password reset has been requested. Please use the link below to reset your password." + Environment.NewLine;
            text += Environment.NewLine;
            text += Settings.RootUrl + "auth/reset?e=" + user.Email + "&t=" + token + Environment.NewLine;

            var html = user.FirstName + Environment.NewLine;
            html += Environment.NewLine;
            html += "A password reset has been requested. Please use the link below to reset your password." + Environment.NewLine;
            html += Environment.NewLine;
            html += Settings.RootUrl + "auth/reset?e=" + user.Email + "&t=" + WebUtility.UrlEncode(token) + Environment.NewLine;

            await emailSender.SendEmailAsync(user.Email, user.FullName, "Password Reset", text, html);

            return Ok();
        }

        [HttpPost("[Action]"), AllowAnonymous]
        public async Task<IActionResult> Reset([FromBody] ResetDTO resetDTO)
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

            await emailSender.SendEmailAsync(user.Email, user.FullName, "Password Reset", body);

            return Ok();
        }

        [HttpPost("[Action]"), AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            throw new Exception("Not Implemented - Register");

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

        public class ProfileModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName { get; set; }
            public Guid UserId { get; set; }
            public List<string> Roles { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public Guid? OrganisationId { get; set; }
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

        public class ChangePasswordDTO
        {
            [Required]
            public string CurrentPassword { get; set; }
            [Required]
            public string NewPassword { get; set; }
            [Required]
            public string ConfirmPassword { get; set; }
        }
    }
}