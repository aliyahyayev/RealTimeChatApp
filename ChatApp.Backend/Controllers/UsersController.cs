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
        // 1. İstifadəçi Girişi və ya Qeydiyyatı (POST: api/users/register)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromQuery] string username, [FromQuery] string email)
        {
            // Eyni adlı istifadəçinin olub-olmadığını yoxlayırıq
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            // Əgər istifadəçi artıq varsa, xəta vermirik! Mövcud istifadəçini geri qaytarırıq (Login mexanizmi)
            if (existingUser != null)
            {
                return Ok(existingUser);
            }

            // Əgər istifadəçi yoxdursa, yeni qeydiyyat yaradırıq (Register mexanizmi)
            var user = new User
            {
                Username = username,
                Email = email
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user); // Yeni yaranmış istifadəçini qaytarır
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