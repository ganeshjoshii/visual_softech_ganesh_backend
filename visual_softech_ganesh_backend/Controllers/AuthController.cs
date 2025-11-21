using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using visual_softech_ganesh_backend.InterFace;
using visual_softech_ganesh_backend.Models;
using visual_softech_ganesh_backend.Services;

namespace visual_softech_ganesh_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthInterface authService) : ControllerBase
    {

        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await authService.RegisterAsync(request);
            if (user == null)
            {
                return BadRequest("User Is already Exists.");
            }
            
            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<TokenResponseDto>> login(UserDto request)
        {
            var result = await authService.LoginAsync(request);
            if (result == null)
            {
                return BadRequest("Invalid UserName and Password :");
            }
            return Ok(result);
        }

        [HttpPost("refresh-token")]

        public async Task<ActionResult<RefreshTokenRequestDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await authService.RefreshTokenAsync(request);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
            {
                return Unauthorized("Invalid Refresh Token");
            }
            return Ok(result);
        }

        [Authorize]
        [HttpGet("Auth-endpoint")]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("You are Authenticated!");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You are Admin.");
        }
    }
}
