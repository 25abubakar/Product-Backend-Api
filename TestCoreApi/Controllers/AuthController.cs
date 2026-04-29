using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestCoreApi.Models.DTOs;
using TestCoreApi.Services;

namespace TestCoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            var (success, error, result) = await _auth.RegisterAsync(dto);

            if (!success)
                return Conflict(new { message = error });

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            var (success, error, result) = await _auth.LoginAsync(dto);

            if (!success)
                return Unauthorized(new { message = error });

            return Ok(result);
        }
    }
}
