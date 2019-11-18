using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEB.Models;

namespace WEB.Controllers
{
    //[ApiController, Authorize(AuthenticationSchemes = OpenIddictValidationDefaults.AuthenticationScheme)]
    public class BaseMvcController : ControllerBase
    {
        internal ApplicationDbContext db;
        internal UserManager<User> userManager;
        internal Settings Settings;

        internal BaseMvcController(ApplicationDbContext applicationDbContext, UserManager<User> userManager, Settings settings)
        {
            db = applicationDbContext;
            this.userManager = userManager;
            this.Settings = settings;
        }
    }
}