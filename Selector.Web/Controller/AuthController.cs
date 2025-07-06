using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Selector.Model;
using Selector.Web.Auth;
using Selector.Web.Extensions;

namespace Selector.Web.Controller;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]/token")]
public class AuthController : BaseAuthController
{
    private readonly JwtTokenService _tokenService;

    public AuthController(ApplicationDbContext context, IAuthorizationService auth,
        UserManager<ApplicationUser> userManager, ILogger<BaseAuthController> logger,
        JwtTokenService tokenService) : base(context, auth, userManager, logger)
    {
        _tokenService = tokenService;
    }

    public class TokenModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Token([FromForm] TokenModel model)
    {
        Activity.Current?.Enrich(HttpContext);

        var user = await UserManager.GetUserAsync(User);

        if (user is null) // user isn't logged in, use parameter creds
        {
            if (model.Username is null)
            {
                return BadRequest("No username provided");
            }

            if (model.Password is null)
            {
                return BadRequest("No password provided");
            }

            var normalUsername = model.Username.Trim().ToUpperInvariant();
            user = await Context.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalUsername);

            if (user is null)
            {
                return NotFound("user not found");
            }

            if (!await UserManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized("credentials failed");
            }
        }

        var token = await _tokenService.CreateJwtToken(user);
        var tokenHandler = new JwtSecurityTokenHandler();

        return Ok(new Dictionary<string, string>
        {
            { "token", tokenHandler.WriteToken(token) }
        });
    }
}