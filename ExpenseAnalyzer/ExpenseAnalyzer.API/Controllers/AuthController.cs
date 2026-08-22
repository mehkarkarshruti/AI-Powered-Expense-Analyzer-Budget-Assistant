using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterUserDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (result == null)
            {
                return Conflict(new
                {
                    message = "A user with this email already exists."
                });
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            return Ok(result);
        }
    }
}