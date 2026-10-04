using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Core.DTOs.Request;
using KeyStone_Identity.Core.DTOs.Response;
using KeyStone_Identity.Core.Enums;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
        [EnableRateLimiting("per-ip")]
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
        [EnableRateLimiting("per-ip")]
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

        [HttpPost("Resend-Verification")]
        [EnableRateLimiting("per-ip")]
        public async Task<ActionResult<string>> ResendEmailVerification(string username)
        {
            if(string.IsNullOrEmpty(username))
            {
                return BadRequest("Username cannot be empty");
            }
            var result = await _authService.ResendEmailVerification(username);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult> Me()
        {
            return Ok("You are authenticated");
        }

        [Authorize(Policy = Policies.CanViewAuditLogs)]
        [HttpPost("AuditHistory")]
        public async Task<ActionResult<List<AuditHistoryResponse>>> AuditHistory(AuditHistoryRequest request)
        {
            var result = await _authService.GetAuditHistory(request);
            return Ok(result);
        }
    }
}
