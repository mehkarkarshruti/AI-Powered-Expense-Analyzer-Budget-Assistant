using ExpenseAnalyzer.API.Data;
using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
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

        // PUT: api/users/1   //Update User
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User Not Found."
                });
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != id);

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "Another user already uses this email."
                });
            }

            user.Name = dto.Name;
            user.Email = dto.Email;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
