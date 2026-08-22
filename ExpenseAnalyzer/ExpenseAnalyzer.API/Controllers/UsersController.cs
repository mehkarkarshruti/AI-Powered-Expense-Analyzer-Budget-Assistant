using ExpenseAnalyzer.API.Data;
using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ExpenseAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]   //every endpoint inside this controller now require a valid JWT
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;  //access to database
        public UsersController(AppDbContext context)  //dependency injection
        {
            _context = context;
        }

        // GET: api/users   //Get all users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _context.Users.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email
            }).ToListAsync();

            return Ok(users);
        }

        // GET: api/users/1   //Get a particular user
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _context.Users.Where(u => u.UserId == id).Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email
            }).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User Not Found."
                });
            }

            return Ok(user);
        }

        [HttpGet("me")]    //API decides who the user is
        public async Task<ActionResult<UserResponseDto>> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);   //retrieves the jwt token

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => new UserResponseDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPut("me")]  //secure update endpoint   //user can edit only their profile
        public async Task<IActionResult> UpdateMyProfile(UpdateUserDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound();

            bool emailExists = await _context.Users.AnyAsync(
                u => u.Email == dto.Email &&
                     u.UserId != userId);

            if (emailExists)
                return Conflict(new
                {
                    message = "Email already in use."
                });

            user.Name = dto.Name;
            user.Email = dto.Email;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
