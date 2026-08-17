using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Core.DTOs.Response;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KeyStone_Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            this._authService = authService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserRegistrationResponseDTO>> Register(UserRegistrationDTO user)
        {
            if (user == null)
            {
                return BadRequest("User cannot be empty");
            }
            var result = await _authService.RegisterUser(user);
            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<JWTAuthResult>> Login(LoginDTO user)
        {
            if (user == null)
            {
                return BadRequest("User cannot be empty");
            }
             var result = await _authService.Login(user);
            return Ok(result);
        }

        [HttpPost("Refresh")]
        public async Task<ActionResult<JWTAuthResult>> Refresh(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Refresh token cannot be empty");
            }
            var result = await _authService.Refresh(token);
            return Ok(result);
        }

        [HttpGet("Verify-Email")]
        public async Task<ActionResult<string>> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Could not verify email");
            }
            var result = await _authService.ActivateAccount(token);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult> Me()
        {
            return Ok("You are authenticated");
        }
    }
}
