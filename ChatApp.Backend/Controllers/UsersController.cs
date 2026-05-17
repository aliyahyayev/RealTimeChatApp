using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatApp.Backend.Data;
using ChatApp.Backend.Models;

namespace ChatApp.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext _context)
        {
            this._context = _context;
        }

        // 1. Yeni İstifadəçi Qeydiyyatı (POST: api/users/register)
        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string email)
        {
            // Eyni adlı istifadəçinin olub-olmadığını yoxlayaq
            var userExists = await _context.Users.AnyAsync(u => u.Username == username);
            if (userExists)
            {
                return BadRequest("Bu istifadəçi adı artıq götürülüb.");
            }

            var user = new User
            {
                Username = username,
                Email = email
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user); // Bizə Id-si yaranmış istifadəçini qaytaracaq (Məsələn, Id: 1)
        }

        // 2. Bütün İstifadəçilərin Siyahısı (GET: api/users)
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // 3. Bazaya yazılan Mesajların Tarixçəsi (GET: api/users/messages)
        // Bununla yoxlayacağıq ki, SignalR vasitəsilə gələn mesajlar RAM bazaya düşür ya yox
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.Messages
                .Include(m => m.User) // Mesajı yazan istifadəçinin məlumatlarını da gətirsin
                .Select(m => new
                {
                    m.Id,
                    Username = m.User.Username,
                    m.Content,
                    m.Timestamp
                })
                .ToListAsync();

            return Ok(messages);
        }
    }
}